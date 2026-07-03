namespace Web.Models.Modelo_Productos.InventarioModels
{
    public class InventarioModel
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string Producto { get; set; } = string.Empty;
        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }
}
