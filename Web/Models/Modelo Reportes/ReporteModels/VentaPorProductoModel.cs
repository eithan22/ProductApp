namespace Web.Models.Modelo_Reportes.ReporteModels
{
    public class VentaPorProductoModel
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal Total { get; set; }
    }
}
