using ProductApp.Domian.Common.Base;
using ProductApp.Domian.Common.Enums.EnumsOrden;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductApp.Domian.Entitis
{
    public class Orden : BaseEntity 
    {
        public DateTime Fecha { get; private set; }
        public decimal Total { get; private set; }

        public EstadoOrden Estado { get; private set; }
        public int UsuarioId { get; private set; }
        public Usuario Usuario { get; private set; } = null!;
        public Cliente Cliente { get; private set; } = null!; 

        public int ClienteId { get; set; }

        public List<Pago> Pagos { get; set; } = new List<Pago>();
        public List<OrderDetalle> OrderDetails { get; set; } = new List<OrderDetalle>();


        public Orden(int clienteId , int usuarioId)
        {
            ClienteId = clienteId;
            UsuarioId = usuarioId;
            Total = 0;
            Estado = EstadoOrden.Pendiente;
            Fecha = DateTime.Now;
        }

        public void ActualizarTotal(decimal total)
        {
            Total = total;
        }

        public void CambiarEstado(
            EstadoOrden nuevoEstado)
        {
            Estado = nuevoEstado;
        }



        public decimal CalcularSaldoPendiente(decimal totalPagado)
        {
            return Total - totalPagado;
        }

        public bool EstaCompletamentePagada(decimal totalPagado)
        {
            return CalcularSaldoPendiente(totalPagado) <= 0;
        }

        public void CancelarOrden()
        {
            if (Estado == EstadoOrden.Procesada)
                throw new InvalidOperationException("No se puede cancelar una orden que ya ha sido procesada.");

            if (Estado == EstadoOrden.Cancelada)
                throw new InvalidOperationException("La orden ya está cancelada.");

            if (Estado == EstadoOrden.Pagada)
                throw new InvalidOperationException("No se puede cancelar una orden que ya ha sido pagada.");

            if (Estado == EstadoOrden.Entregada)
                throw new InvalidOperationException("No se puede cancelar una orden que ya ha sido entregada.");

            Estado = EstadoOrden.Cancelada;
        }
    }


}
