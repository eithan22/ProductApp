namespace Web.Models.Modelo_Reportes.ReporteModels
{
    public class InventarioActualModel
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int CantidadActual { get; set; }
        public int CantidadMinima { get; set; }
        public bool StockBajo { get; set; }
    }
}
