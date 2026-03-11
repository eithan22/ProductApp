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
    }
}
