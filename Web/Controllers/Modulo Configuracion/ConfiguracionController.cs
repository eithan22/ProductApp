using Microsoft.AspNetCore.Mvc;
using Web.Models.Modelo_Configuracion;
using Web.Services.Interfaces.ServicesHttp.Modulo_Configuracion;

namespace Web.Controllers.Modulo_Configuracion
{
    public class ConfiguracionController : Controller
    {
        private readonly IConfiguracionHttpServices _configuracionHttpServices;

        public ConfiguracionController(IConfiguracionHttpServices configuracionHttpServices)
        {
            _configuracionHttpServices = configuracionHttpServices;
        }

        public async Task<ActionResult> Index()
        {
            var result = await _configuracionHttpServices.ObtenerAsync();
            return View(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(ConfiguracionModel model)
        {
            try
            {
                await _configuracionHttpServices.ActualizarAsync(model);

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
    }
}
