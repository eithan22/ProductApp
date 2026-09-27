using Microsoft.AspNetCore.Mvc;
using Web.Models.Modelo_Configuracion;
using Web.Services.Interfaces.ServicesHttp.Modulo_Configuracion;

namespace Web.Controllers.Modulo_Configuracion
{
    public class ConfiguracionController : Controller
    {
        private readonly IConfiguracionHttpServices _configuracionHttpServices;
        private readonly ILogger<ConfiguracionController> _logger;

        public ConfiguracionController(IConfiguracionHttpServices configuracionHttpServices,
            ILogger<ConfiguracionController> logger)
        {
            _configuracionHttpServices = configuracionHttpServices;
            _logger = logger;
        }

        public async Task<ActionResult> Index()
        {
            if (HttpContext.Session.GetString("ROL") != "Administrador")
            {
                TempData["Error"] = "No tenés permisos para acceder a Configuración.";
                return RedirectToAction("Index", "Home");
            }

            var result = await _configuracionHttpServices.ObtenerAsync();
            return View(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(ConfiguracionModel model, IFormFile? logo)
        {
            if (HttpContext.Session.GetString("ROL") != "Administrador")
            {
                TempData["Error"] = "No tenés permisos para acceder a Configuración.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                await _configuracionHttpServices.ActualizarAsync(model);
                await SubirLogoAsync(logo);

                HttpContext.Session.SetString("EMPRESA", model.NombreEmpresa);
                HttpContext.Session.SetString("MONEDA", model.Moneda);

                TempData["Mensaje"] = "Configuración actualizada correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> QuitarLogo()
        {
            if (HttpContext.Session.GetString("ROL") != "Administrador")
            {
                TempData["Error"] = "No tenés permisos para acceder a Configuración.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                await _configuracionHttpServices.QuitarLogoAsync();
                TempData["Mensaje"] = "Logo quitado correctamente.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // El logo se sube en un segundo paso porque la API solo acepta multipart,
        // no el JSON del formulario de configuración.
        private async Task SubirLogoAsync(IFormFile? logo)
        {
            if (logo is null || logo.Length == 0)
                return;

            try
            {
                await using var contenido = logo.OpenReadStream();
                await _configuracionHttpServices.SubirLogoAsync(contenido, logo.FileName, logo.ContentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo subir el logo de la empresa");
                TempData["Aviso"] = $"La configuración se guardó, pero el logo no se pudo subir: {ex.Message}";
            }
        }
    }
}
