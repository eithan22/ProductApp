namespace Web.Models.Modelo_Productos.ProductoModels
{
    public class CreateProductoModel
    {
        public string Nombre { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public decimal Precio { get; set; }
        public decimal Costo { get; set; }
        public int CategoriaId { get; set; }

        // Opcional: si el select queda vacío, el producto se guarda sin proveedor.
        public int? ProveedorId { get; set; }
    }
}
