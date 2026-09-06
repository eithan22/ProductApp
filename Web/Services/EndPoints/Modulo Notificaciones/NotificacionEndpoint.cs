using Web.Services.Interfaces.IEndPoints.Modulo_Notificaciones;

namespace Web.Services.EndPoints.Modulo_Notificaciones
{
    public class NotificacionEndpoint : INotificacionEndpoint
    {
        public string GetResumen => "Notificacion/GetResumen";
        public string MarcarLeida => "Notificacion/MarcarLeida/";
        public string MarcarTodasLeidas => "Notificacion/MarcarTodasLeidas";
    }
}
