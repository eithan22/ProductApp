using ProductApp.Domian.Common.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductApp.Domian.Entitis
{
    public class OrderDetalle : BaseEntity // clase intermedia n*n
    {
        public int ProductId { get; private set; }
        public Producto Producto { get; private set; } = null!;

        public int Cantidad { get; private set; }
        public decimal PrecioUnitario { get; private set; }

        public decimal Subtotal { get; private set; }

        public int OrdenId { get; private set; }
        public Orden Orden { get; private  set; } = null!;


        public OrderDetalle()
        {
        }


        public OrderDetalle(int productId, int cantidad, decimal precioUnitario, int ordenId)
        {
            ProductId = productId;
            Cantidad = cantidad;
            PrecioUnitario = precioUnitario;
            OrdenId = ordenId;
            ActualizarCantidad(cantidad);
            CalcularSubtotal();

        }


        public void ActualizarCantidad(int nuevaCantidad)
        {
            Cantidad = nuevaCantidad;
            CalcularSubtotal();
        }
        public void CalcularSubtotal()
        {
            Subtotal = Cantidad * PrecioUnitario;

        }
    }
}
