namespace Web.Services.Interfaces.IEndPoints.Modulo_Ventas
{
    public interface IDetalleOrdenEndpoint
    {
        string Create { get; }
        string GetByOrden { get; }
        string Update { get; }
        string Delete { get; }
    }
}
