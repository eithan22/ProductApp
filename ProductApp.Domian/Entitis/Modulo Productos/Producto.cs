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

        public Inventario Inventario { get; private set; } = null!;
        public Categoria Categoria { get; private set; } = null!;

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

            Precio = precio;
            ActualizarFechaModificacion();
        }

        public void CambiarYvalidarCosto(decimal costo)
        {
            if (costo < 0)
                throw new ValidacionDominioException("Costo", "El costo del producto no puede ser negativo.");

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
