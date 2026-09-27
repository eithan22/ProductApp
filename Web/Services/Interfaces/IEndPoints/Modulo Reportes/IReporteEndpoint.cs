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

        string ExportarVentasPorFecha { get; }
        string ExportarVentasPorProducto { get; }
        string ExportarVentasPorVendedor { get; }
        string ExportarInventarioActual { get; }
        string ExportarProductosMasVendidos { get; }
        string ExportarIngresosTotales { get; }
    }
}
