namespace Web.Services.Interfaces.IEndPoints.Modulo_Proveedores
{
    public interface IProveedorEndpoint
    {
        string GetAll { get; }
        string GetById { get; }
        string Create { get; }
        string Update { get; }
        string Disable { get; }
        string Enable { get; }
        string GetBuscar { get; }
    }
}
