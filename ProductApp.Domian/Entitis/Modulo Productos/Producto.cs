using ProductApp.Domian.Common.Base;
using ProductApp.Domian.Common.Enums.EnumsProducto;
using ProductApp.Domian.Common.Exceptions;
using ProductApp.Domian.Common.Exceptions.ExceptionsProducto;

namespace ProductApp.Domian.Entitis
{
    public class Producto : BaseEntity
    {
        public string Nombre { get; private set; } = string.Empty;
        public string Descripcion { get; private set; } = string.Empty;
        public decimal Precio { get; private set; }
        public decimal Costo { get; private set; }
        public EstadoProducto Estado { get; private set; } = EstadoProducto.Activo;
        public int CategoriaId { get; private set; }
        public string? ImagenUrl { get; private set; }

        // Proveedor principal, opcional (RF-3.7.2). Es nullable a propósito: el catálogo
        // ya existente no tiene proveedor asignado y debe seguir funcionando igual.
        public int? ProveedorId { get; private set; }

        public Inventario Inventario { get; private set; } = null!;
        public Categoria Categoria { get; private set; } = null!;
        public Proveedor? Proveedor { get; private set; }

        // Tope por unidad para precio y costo. La columna es decimal(18,2) y aguanta mucho
        // más, pero un monto de siete cifras en un producto de mostrador siempre es un dedazo
        // o un archivo de importación mal armado, y el error se propaga al total de la orden.
        public const decimal MontoMaximo = 999_999.99m;

        protected Producto() { }

        public Producto(string nombre, string descripcion, decimal precio, decimal costo, int categoriaId)
        {
            CambiarYvalidarNombre(nombre);
            CambiarYvalidarDescripcion(descripcion);
            CambiarYvalidarPrecio(precio);
            CambiarYvalidarCosto(costo);
            CambiarYvalidarCategoria(categoriaId);
        }

        // --- Métodos de actualización ---

        public void CambiarYvalidarNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ValidacionDominioException("Nombre", "El nombre del producto no puede estar vacío.");

            Nombre = nombre;
            ActualizarFechaModificacion();
        }

        public void CambiarYvalidarDescripcion(string descripcion)
        {
            if (string.IsNullOrWhiteSpace(descripcion))
                throw new ValidacionDominioException("Descripcion", "La descripción del producto no puede estar vacía.");

            Descripcion = descripcion;
            ActualizarFechaModificacion();
        }

        public void CambiarYvalidarPrecio(decimal precio)
        {
            if (precio < 0)
                throw new PrecioInvalidoException(precio);

            if (precio > MontoMaximo)
                throw new ValidacionDominioException("Precio",
                    $"El precio no puede superar los {MontoMaximo}.");

            Precio = precio;
            ActualizarFechaModificacion();
        }

        public void CambiarYvalidarCosto(decimal costo)
        {
            if (costo < 0)
                throw new ValidacionDominioException("Costo", "El costo del producto no puede ser negativo.");

            if (costo > MontoMaximo)
                throw new ValidacionDominioException("Costo",
                    $"El costo no puede superar los {MontoMaximo}.");

            Costo = costo;
            ActualizarFechaModificacion();
        }

        public void CambiarYvalidarCategoria(int categoriaId)
        {
            if (categoriaId <= 0)
                throw new ValidacionDominioException("CategoriaId", "El id de la categoría debe ser mayor a cero.");

            CategoriaId = categoriaId;
            ActualizarFechaModificacion();
        }

        // Recibe int? y no int porque "sin proveedor" es un valor válido del negocio:
        // mandar null es la forma explícita de dejar el producto sin proveedor asignado.
        public void AsignarProveedor(int? proveedorId)
        {
            if (proveedorId.HasValue && proveedorId.Value <= 0)
                throw new ValidacionDominioException("ProveedorId", "El id del proveedor debe ser mayor a cero.");

            ProveedorId = proveedorId;
            ActualizarFechaModificacion();
        }

        public void QuitarProveedor()
        {
            ProveedorId = null;
            ActualizarFechaModificacion();
        }

        public void AsignarImagen(string imagenUrl)
        {
            if (string.IsNullOrWhiteSpace(imagenUrl))
                throw new ValidacionDominioException("ImagenUrl", "La referencia de la imagen no puede estar vacía.");

            ImagenUrl = imagenUrl;
            ActualizarFechaModificacion();
        }

        public void QuitarImagen()
        {
            ImagenUrl = null;
            ActualizarFechaModificacion();
        }

        // --- Ciclo de vida ---

        public void DesactivarProducto()
        {
            if (Estado == EstadoProducto.Inactivo)
                throw new EstadoInvalidoException("Producto", Estado.ToString(), "Desactivar");

            Estado = EstadoProducto.Inactivo;
            ActualizarFechaModificacion();
        }

        public void ActivarProducto()
        {
            if (Estado == EstadoProducto.Activo)
                throw new EstadoInvalidoException("Producto", Estado.ToString(), "Activar");

            Estado = EstadoProducto.Activo;
            ActualizarFechaModificacion();
        }
    }
}
