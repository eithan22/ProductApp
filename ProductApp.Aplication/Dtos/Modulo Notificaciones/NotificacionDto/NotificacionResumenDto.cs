namespace ProductApp.Aplication.Dtos.Modulo_Notificaciones
{
    public class NotificacionResumenDto
    {
        public List<NotificacionResponseDto> Recientes { get; set; } = new();
        public int NoLeidas { get; set; }
    }
}
