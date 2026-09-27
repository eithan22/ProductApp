namespace Web.Models.Modelo_Productos.ProductoModels
{
    public class ProductoModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public decimal Precio { get; set; }
        public decimal Costo { get; set; }
        public string Estado { get; set; } = "";
        public string? Categoria { get; set; }
        public string? ImagenUrl { get; set; }

        // Null cuando el producto no tiene proveedor asignado: la asignación es opcional.
        public int? ProveedorId { get; set; }
        public string? Proveedor { get; set; }

        public int? StockActual { get; set; }
        public int? StockMinimo { get; set; }
    }
}
