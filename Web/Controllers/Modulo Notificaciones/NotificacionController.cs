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
        public async Task<IActionResult> MarcarLeida(int id, string? retorno)
        {
            await _notificacionHttpServices.MarcarComoLeidaAsync(id);
            return VolverAOrigen(retorno);
        }

        [HttpPost]
        public async Task<IActionResult> MarcarTodasLeidas(string? retorno)
        {
            await _notificacionHttpServices.MarcarTodasComoLeidasAsync();
            return VolverAOrigen(retorno);
        }

        // La página de retorno la manda la propia vista en un campo oculto, no el header
        // Referer: la Web responde con Referrer-Policy: no-referrer, así que ese header
        // llegaba siempre vacío y "volver a donde estabas" nunca funcionó. Url.IsLocalUrl
        // descarta cualquier destino absoluto, así que un valor manipulado no puede sacar
        // al usuario del sitio; si no es local, se cae a Home.
        private IActionResult VolverAOrigen(string? retorno)
            => string.IsNullOrEmpty(retorno) || !Url.IsLocalUrl(retorno)
                ? RedirectToAction("Index", "Home")
                : LocalRedirect(retorno);
    }
}
