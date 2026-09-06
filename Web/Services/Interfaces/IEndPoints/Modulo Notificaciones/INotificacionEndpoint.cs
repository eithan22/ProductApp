namespace Web.Services.Interfaces.IEndPoints.Modulo_Notificaciones
{
    public interface INotificacionEndpoint
    {
        string GetResumen { get; }
        string MarcarLeida { get; }
        string MarcarTodasLeidas { get; }
    }
}
