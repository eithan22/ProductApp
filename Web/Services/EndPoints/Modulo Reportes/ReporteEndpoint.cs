using Web.Services.Interfaces.IEndPoints.Modulo_Reportes;

namespace Web.Services.EndPoints.Modulo_Reportes
{
    public class ReporteEndpoint : IReporteEndpoint
    {
        public string VentasPorFecha => "Reporte/VentasPorFecha";
        public string VentasPorProducto => "Reporte/VentasPorProducto";
        public string VentasPorVendedor => "Reporte/VentasPorVendedor";
        public string InventarioActual => "Reporte/InventarioActual";
        public string ProductosMasVendidos => "Reporte/ProductosMasVendidos";
        public string IngresosTotales => "Reporte/IngresosTotales";
    }
}
