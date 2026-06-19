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

        public string MetodoPago { get; set; } = string.Empty;

        public string EstadoPago { get; set; } = string.Empty;

        public DateTime FechaPago { get; set; }

        public decimal SaldoPendiente { get; set; }
    }
}
