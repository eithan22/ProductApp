namespace Web.Models.Modelo_Productos.InventarioModels
{
    public class InventarioModel
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public string Producto { get; set; } = string.Empty;

        // Viene del producto, no del inventario: permite filtrar y agrupar el stock
        // bajo por quién repone, sin pedir cada producto por separado.
        public int? ProveedorId { get; set; }
        public string? Proveedor { get; set; }

        public int StockActual { get; set; }
        public int StockMinimo { get; set; }
        public DateTime FechaActualizacion { get; set; }
    }
}
