using ProductApp.Domian.Common.Enums.EnumsPago;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Dtos.PagoDto
{
    public class PagoResponseDto
    {
        public int Id { get; set; }

        public decimal Monto { get; set; }

        public MetodoPago MetodoPago { get; set; }

        public EstadoPago EstadoPago { get; set; }

        public DateTime FechaPago { get; set; }

        public decimal SaldoPendiente { get; set; }
    }
}
