namespace Web.Models.Modelo_Notificaciones
{
    public class NotificacionResumenModel
    {
        public List<NotificacionModel> Recientes { get; set; } = new();
        public int NoLeidas { get; set; }
    }
}
