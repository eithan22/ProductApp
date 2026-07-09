namespace ProductApp.Aplication.Dtos.ReporteDto
{
    public class InventarioActualDto
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int CantidadActual { get; set; }
        public int CantidadMinima { get; set; }
        public bool StockBajo { get; set; }
    }
}
