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

        public string ExportarVentasPorFecha => "Reporte/VentasPorFecha/Exportar";
        public string ExportarVentasPorProducto => "Reporte/VentasPorProducto/Exportar";
        public string ExportarVentasPorVendedor => "Reporte/VentasPorVendedor/Exportar";
        public string ExportarInventarioActual => "Reporte/InventarioActual/Exportar";
        public string ExportarProductosMasVendidos => "Reporte/ProductosMasVendidos/Exportar";
        public string ExportarIngresosTotales => "Reporte/IngresosTotales/Exportar";
    }
}
