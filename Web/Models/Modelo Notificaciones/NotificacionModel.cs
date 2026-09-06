namespace Web.Models.Modelo_Notificaciones
{
    public class NotificacionModel
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public bool Leida { get; set; }
        public DateTime CreadoEn { get; set; }
    }
}
