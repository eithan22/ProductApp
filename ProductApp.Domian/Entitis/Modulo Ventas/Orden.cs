using ProductApp.Domian.Common.Base;
using ProductApp.Domian.Common.Enums.EnumsOrden;
using ProductApp.Domian.Common.Exceptions;

namespace ProductApp.Domian.Entitis
{
    public class Orden : BaseEntity
    {
        public DateTime Fecha { get; private set; }
        public decimal Total { get; private set; }
        public EstadoOrden Estado { get; private set; }
        public int UsuarioId { get; private set; }
        public int ClienteId { get; private set; }

        public Usuario Usuario { get; private set; } = null!;
        public Cliente Cliente { get; private set; } = null!;

        public IReadOnlyList<Pago> Pagos { get; private set; } = new List<Pago>();
        public IReadOnlyList<OrdenDetalle> Detalles { get; private set; } = new List<OrdenDetalle>();

        // Transiciones de estado permitidas
        private static readonly Dictionary<EstadoOrden, EstadoOrden[]> _transicionesValidas = new()
        {
            { EstadoOrden.Pendiente,  new[] { EstadoOrden.Procesada, EstadoOrden.Pagada, EstadoOrden.Cancelada } },
            { EstadoOrden.Procesada,  new[] { EstadoOrden.Pagada, EstadoOrden.Cancelada } },
            { EstadoOrden.Pagada,     new[] { EstadoOrden.Entregada } },
            { EstadoOrden.Cancelada,  Array.Empty<EstadoOrden>() },
            { EstadoOrden.Entregada,  Array.Empty<EstadoOrden>() },
        };

        protected Orden() { }

        public Orden(int clienteId, int usuarioId)
        {
            if (clienteId <= 0)
                throw new ValidacionDominioException("ClienteId", "El id del cliente no es válido.");

            if (usuarioId <= 0)
                throw new ValidacionDominioException("UsuarioId", "El id del usuario no es válido.");

            ClienteId = clienteId;
            UsuarioId = usuarioId;
            Total = 0;
            Estado = EstadoOrden.Pendiente;
            Fecha = DateTime.UtcNow;
        }

        // --- Comportamiento ---

        public void CambiarEstado(EstadoOrden nuevoEstado)
        {
            if (!_transicionesValidas.TryGetValue(Estado, out var permitidos)
                || !permitidos.Contains(nuevoEstado))
                throw new EstadoInvalidoException(
                    "Orden", Estado.ToString(), $"cambiar a {nuevoEstado}");

            Estado = nuevoEstado;
            ActualizarFechaModificacion();
        }

        public void CancelarOrden()
        {
            CambiarEstado(EstadoOrden.Cancelada);
        }

        public void ActualizarTotal(decimal total)
        {
            if (total < 0)
                throw new ValidacionDominioException("Total", "El total de la orden no puede ser negativo.");

            Total = total;
            ActualizarFechaModificacion();
        }

        // --- Cálculos de saldo ---

        public decimal CalcularSaldoPendiente(decimal totalPagado) => Total - totalPagado;

        public bool EstaCompletamentePagada(decimal totalPagado) => CalcularSaldoPendiente(totalPagado) <= 0;
    }
}
