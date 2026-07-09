namespace Web.Models.Modelo_Reportes.ReporteModels
{
    public class ReporteDashboardModel
    {
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public IngresosTotalesModel? Ingresos { get; set; }
        public List<VentaPorFechaModel> VentasPorFecha { get; set; } = new();
        public List<ProductoMasVendidoModel> TopProductos { get; set; } = new();
        public int TotalProductosInventario { get; set; }
        public int ProductosStockBajo { get; set; }
    }
}
