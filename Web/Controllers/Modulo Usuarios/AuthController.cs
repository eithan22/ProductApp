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



        // GET: AuthController/Login
        [HttpGet]
        public ActionResult Login()
        {
            // Si ya hay sesión activa no tiene sentido mostrar el formulario:
            // el layout lo renderizaría dentro del shell de la aplicación.
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("TOKEN")))
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        // POST: AuthController/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginModel model)
        {
            // Sin esto se llama a la API con el modelo vacío y su respuesta 400
            // se agrega como un error genérico encima de los mensajes de campo.
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _authHttpServices.Login(model);

                // Se descarta lo que hubiera en la sesión antes de escribir el token nuevo: si
                // alguien logró dejar datos plantados ahí antes del login, no sobreviven al
                // inicio de sesión. No rota el id de sesión (el middleware de ASP.NET Core no
                // lo permite); eso queda como deuda documentada.
                HttpContext.Session.Clear();

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
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }


        // POST y con token antifalsificación: por GET, un tercero podía cerrarle la sesión a
        // cualquiera con solo hacerle abrir un enlace a esta ruta desde otro sitio.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // elimina el token

            return RedirectToAction("Login", "Auth");
        }




    }
}
