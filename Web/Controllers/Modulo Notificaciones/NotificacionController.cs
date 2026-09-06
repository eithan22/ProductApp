using Microsoft.AspNetCore.Mvc;
using Web.Services.Interfaces.ServicesHttp.Modulo_Notificaciones;

namespace Web.Controllers.Modulo_Notificaciones
{
    public class NotificacionController : Controller
    {
        private readonly INotificacionHttpServices _notificacionHttpServices;

        public NotificacionController(INotificacionHttpServices notificacionHttpServices)
        {
            _notificacionHttpServices = notificacionHttpServices;
        }

        [HttpPost]
        public async Task<IActionResult> MarcarLeida(int id)
        {
            await _notificacionHttpServices.MarcarComoLeidaAsync(id);
            return VolverAOrigen();
        }

        [HttpPost]
        public async Task<IActionResult> MarcarTodasLeidas()
        {
            await _notificacionHttpServices.MarcarTodasComoLeidasAsync();
            return VolverAOrigen();
        }

        private IActionResult VolverAOrigen()
        {
            var referer = Request.Headers.Referer.ToString();
            return string.IsNullOrEmpty(referer)
                ? RedirectToAction("Index", "Home")
                : Redirect(referer);
        }
    }
}
