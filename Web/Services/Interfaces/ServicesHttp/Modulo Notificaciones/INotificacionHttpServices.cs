using Web.Models.Modelo_Notificaciones;

namespace Web.Services.Interfaces.ServicesHttp.Modulo_Notificaciones
{
    public interface INotificacionHttpServices
    {
        Task<NotificacionResumenModel> ObtenerResumenAsync(int cantidad = 10);
        Task MarcarComoLeidaAsync(int id);
        Task MarcarTodasComoLeidasAsync();
    }
}
