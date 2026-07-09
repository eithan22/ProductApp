namespace ProductApp.Aplication.Dtos.ReporteDto
{
    public class VentaPorProductoDto
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal Total { get; set; }
    }
}
