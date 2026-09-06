using Web.Models.Modelo_Notificaciones;
using Web.Services.Interfaces.IBase;
using Web.Services.Interfaces.IEndPoints.Modulo_Notificaciones;
using Web.Services.Interfaces.ServicesHttp.Modulo_Notificaciones;

namespace Web.Services.ServicesHttp.Modulo_Notificaciones
{
    public class NotificacionHttpServices : INotificacionHttpServices
    {
        private readonly IBaseHttpServices _baseHttpServices;
        private readonly INotificacionEndpoint _notificacionEndpoint;

        public NotificacionHttpServices(IBaseHttpServices baseHttpServices, INotificacionEndpoint notificacionEndpoint)
        {
            _baseHttpServices = baseHttpServices;
            _notificacionEndpoint = notificacionEndpoint;
        }

        public async Task<NotificacionResumenModel> ObtenerResumenAsync(int cantidad = 10)
        {
            return await _baseHttpServices.GetAsync<NotificacionResumenModel>($"{_notificacionEndpoint.GetResumen}?cantidad={cantidad}");
        }

        public async Task MarcarComoLeidaAsync(int id)
        {
            await _baseHttpServices.PatchAsync<object, object>($"{_notificacionEndpoint.MarcarLeida}{id}", new { });
        }

        public async Task MarcarTodasComoLeidasAsync()
        {
            await _baseHttpServices.PatchAsync<object, object>(_notificacionEndpoint.MarcarTodasLeidas, new { });
        }
    }
}
