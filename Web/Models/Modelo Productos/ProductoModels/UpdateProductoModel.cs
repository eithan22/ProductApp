namespace Web.Models.Modelo_Productos.ProductoModels
{
    public class UpdateProductoModel
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public decimal Precio { get; set; }
        public decimal Costo { get; set; }
        public int CategoriaId { get; set; }

        // Mandar null es la forma de dejar el producto sin proveedor.
        public int? ProveedorId { get; set; }
    }
}
