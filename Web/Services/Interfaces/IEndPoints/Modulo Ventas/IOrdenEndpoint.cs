namespace Web.Services.Interfaces.IEndPoints.Modulo_Ventas
{
    public interface IOrdenEndpoint
    {
        string Create { get; }
        string GetAll { get; }
        string GetById { get; }
        string GetByCliente { get; }
        string GetByUsuario { get; }
        string GetByFecha { get; }
        string CambiarEstado { get; }
        string Cancelar { get; }
    }
}
