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
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }

        public EstadoOrden Estado { get; set; }
        public int UsuarioId { get; set; }
        public Usuario Usuario { get; set; } = null!;
        public Cliente Cliente { get; set; } = null!; 

        public int ClienteId { get; set; }

        public Pago Pago { get; set; } = null!;
        public List<OrderDetalle> OrderDetails { get; set; } = new List<OrderDetalle>();


        /*
        public Orden(int clienteId, decimal total)
        {
            ClienteId = clienteId;
            Total = total;
            Estado = EstadoOrden.Pendiente;
            Fecha = DateTime.Now;
        }

        */

        public void CancelarOrden()
        {
            if (Estado == EstadoOrden.Procesada)
            {
                throw new InvalidOperationException("No se puede cancelar una orden que ya ha sido procesada.");
            }

            if (Estado == EstadoOrden.Cancelada)
            {
                throw new InvalidOperationException("La orden ya está cancelada.");
            }


            Estado = EstadoOrden.Cancelada;
        }
    }


}
