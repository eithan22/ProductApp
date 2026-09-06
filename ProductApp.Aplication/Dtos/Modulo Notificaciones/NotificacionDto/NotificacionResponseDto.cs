namespace ProductApp.Aplication.Dtos.Modulo_Notificaciones
{
    public class NotificacionResponseDto
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public bool Leida { get; set; }
        public DateTime CreadoEn { get; set; }
    }
}
