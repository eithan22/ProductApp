using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Web.Models.Modelo_Usuarios.UsuarioModels.AuthModel;
using Web.Services.Interfaces.ServicesHttp.Modulo_Usuarios;

namespace Web.Controllers.Modulo_Usuarios
{
    public class AuthController : Controller
    {
        private readonly IAuthHttpServices _authHttpServices;

        public AuthController(IAuthHttpServices authHttpServices)
        {
            _authHttpServices = authHttpServices;
        }



        // GET: AuthController
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterModel model)
        {
            try
            {
                var reuslt = await _authHttpServices.Register(model);
                return RedirectToAction(("Login"));
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }




        // GET: AuthController/Create
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        // POST: AuthController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginModel model)
        {
            try
            {
               
                    var result = await _authHttpServices.Login(model);

                    // Guardar el token en la sesión
                    HttpContext.Session.SetString("TOKEN", result.Token);

                    return RedirectToAction("Index", "Home");
                

                
                
            }
            catch(Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // 🔐 elimina el token

            return RedirectToAction("Login", "Auth");
        }




    }
}
