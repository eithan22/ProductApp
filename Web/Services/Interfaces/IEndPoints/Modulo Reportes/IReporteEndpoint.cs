namespace Web.Services.Interfaces.IEndPoints.Modulo_Reportes
{
    public interface IReporteEndpoint
    {
        string VentasPorFecha { get; }
        string VentasPorProducto { get; }
        string VentasPorVendedor { get; }
        string InventarioActual { get; }
        string ProductosMasVendidos { get; }
        string IngresosTotales { get; }
    }
}
