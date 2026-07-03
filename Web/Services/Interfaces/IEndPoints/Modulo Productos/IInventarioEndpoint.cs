namespace Web.Services.Interfaces.IEndPoints.Modulo_Productos
{
    public interface IInventarioEndpoint
    {
        string GetAll { get; }
        string GetInventarioPorProducto { get; }
        string GetStockBajo { get; }
        string AgregarStock { get; }
        string DescontarStock { get; }
        string AjustarInventario { get; }
    }
}
