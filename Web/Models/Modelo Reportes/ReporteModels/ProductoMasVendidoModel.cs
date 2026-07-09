namespace Web.Models.Modelo_Reportes.ReporteModels
{
    public class ProductoMasVendidoModel
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
    }
}
