using ProductApp.Domian.Common.Base;
using ProductApp.Domian.Common.Exceptions;

namespace ProductApp.Domian.Entitis
{
    public class OrdenDetalle : BaseEntity
    {
        public int ProductId { get; private set; }
        public int Cantidad { get; private set; }
        public decimal PrecioUnitario { get; private set; }
        public decimal Subtotal { get; private set; }
        public int OrdenId { get; private set; }

        public Producto Producto { get; private set; } = null!;
        public Orden Orden { get; private set; } = null!;

        protected OrdenDetalle() { }

        public OrdenDetalle(int productId, int cantidad, decimal precioUnitario, int ordenId)
        {
            if (productId <= 0)
                throw new ValidacionDominioException("ProductId", "El id del producto no es válido.");

            if (ordenId <= 0)
                throw new ValidacionDominioException("OrdenId", "El id de la orden no es válido.");

            if (precioUnitario < 0)
                throw new ValidacionDominioException("PrecioUnitario", "El precio unitario no puede ser negativo.");

            ProductId = productId;
            OrdenId = ordenId;
            PrecioUnitario = precioUnitario;
            ActualizarCantidad(cantidad);
        }

        // --- Comportamiento ---

        public void ActualizarCantidad(int nuevaCantidad)
        {
            if (nuevaCantidad <= 0)
                throw new ValidacionDominioException("Cantidad", "La cantidad debe ser mayor a cero.");

            Cantidad = nuevaCantidad;
            CalcularSubtotal();
            ActualizarFechaModificacion();
        }

        private void CalcularSubtotal()
        {
            Subtotal = Cantidad * PrecioUnitario;
        }
    }
}
