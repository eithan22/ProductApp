using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Web.Models.Modelo_Usuarios.UsuarioModels.AuthModel;
using Web.Services.Interfaces.ServicesHttp.Modulo_Configuracion;
using Web.Services.Interfaces.ServicesHttp.Modulo_Usuarios;

namespace Web.Controllers.Modulo_Usuarios
{
    public class AuthController : Controller
    {
        private readonly IAuthHttpServices _authHttpServices;
        private readonly IConfiguracionHttpServices _configuracionHttpServices;

        public AuthController(IAuthHttpServices authHttpServices, IConfiguracionHttpServices configuracionHttpServices)
        {
            _authHttpServices = authHttpServices;
            _configuracionHttpServices = configuracionHttpServices;
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

                    // Guardar el token y el rol en la sesión
                    HttpContext.Session.SetString("TOKEN", result.Token);
                    HttpContext.Session.SetString("ROL", result.Usuario.RolUsuario);
                    HttpContext.Session.SetString("NOMBRE", result.Usuario.Nombre);

                    try
                    {
                        var configuracion = await _configuracionHttpServices.ObtenerAsync();
                        HttpContext.Session.SetString("EMPRESA", configuracion.NombreEmpresa);
                        HttpContext.Session.SetString("MONEDA", configuracion.Moneda);
                    }
                    catch
                    {
                        // La configuración es informativa; si falla, seguimos con los valores por defecto.
                    }

                    if (result.DebeCambiarPassword)
                    {
                        TempData["Aviso"] = "Debes cambiar tu contraseña antes de continuar.";
                        return RedirectToAction("CambiarPassword", "Usuario");
                    }

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
