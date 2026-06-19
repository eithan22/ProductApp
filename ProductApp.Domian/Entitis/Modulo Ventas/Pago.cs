using ProductApp.Domian.Common.Base;
using ProductApp.Domian.Common.Enums.EnumsPago;
using ProductApp.Domian.Common.Exceptions;

namespace ProductApp.Domian.Entitis
{
    public class Pago : BaseEntity
    {
        public decimal Monto { get; private set; }
        public DateTime FechaPago { get; private set; }
        public EstadoPago Estado { get; private set; }
        public MetodoPago MetodoPago { get; private set; }
        public int OrdenId { get; private set; }

        public Orden Orden { get; private set; } = null!;

        protected Pago() { }

        public Pago(int ordenId, decimal monto, MetodoPago metodoPago)
        {
            if (ordenId <= 0)
                throw new ValidacionDominioException("OrdenId", "El id de la orden no es válido.");

            if (monto <= 0)
                throw new ValidacionDominioException("Monto", "El monto del pago debe ser mayor a cero.");

            OrdenId = ordenId;
            Monto = monto;
            MetodoPago = metodoPago;
            FechaPago = DateTime.UtcNow;
            Estado = EstadoPago.Pendiente;
        }

        // --- Ciclo de vida ---

        public void MarcarComoCompletado()
        {
            if (Estado != EstadoPago.Pendiente)
                throw new EstadoInvalidoException("Pago", Estado.ToString(), "MarcarComoCompletado");

            Estado = EstadoPago.Completado;
            ActualizarFechaModificacion();
        }

        public void MarcarComoFallido()
        {
            if (Estado != EstadoPago.Pendiente)
                throw new EstadoInvalidoException("Pago", Estado.ToString(), "MarcarComoFallido");

            Estado = EstadoPago.Fallido;
            ActualizarFechaModificacion();
        }
    }
}
