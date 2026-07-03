using Web.Services.Interfaces.IEndPoints.Modulo_Productos;

namespace Web.Services.EndPoints.Modulo_Productos
{
    public class ProductoEndpoint : IProductoEndpoint
    {
        public string GetAll => "Producto/GetAllProductos";
        public string GetById => "Producto/GetProductoById/";
        public string Create => "Producto/CreateProducto";
        public string Update => "Producto/UpdateProducto/";
        public string Disable => "Producto/DisableProducto/";
        public string Enable => "Producto/EnableProducto/";
        public string Buscar => "Producto/BuscarProductos";
    }
}
