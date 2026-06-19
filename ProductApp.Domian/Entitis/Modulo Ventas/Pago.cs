using ProductApp.Domian.Common.Base;
using ProductApp.Domian.Common.Enums.EnumsPago;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Domian.Entitis
{
    public class Pago : BaseEntity
    {
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }

        public EstadoPago Estado { get; set; }
        public MetodoPago MetodoPago { get; set; }

        public int OrdenId { get; set; }

        public Orden Orden { get; set; } = null!;



        protected Pago() { }

        public Pago(int ordenId, decimal monto, MetodoPago metodoPago)
        {
            OrdenId = ordenId;
            Monto = monto;
            MetodoPago = metodoPago;
            FechaPago = DateTime.Now;
            Estado = EstadoPago.Pendiente;
        }

        public void MarcarComoCompletado()
        {
            if (Estado == EstadoPago.Completado)
                throw new InvalidOperationException("El pago ya está completado.");
            Estado = EstadoPago.Completado;
        }
    }

}
