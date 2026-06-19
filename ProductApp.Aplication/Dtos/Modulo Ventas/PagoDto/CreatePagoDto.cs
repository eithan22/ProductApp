using ProductApp.Domian.Common.Enums.EnumsPago;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Dtos.PagoDto
{
    public class CreatePagoDto
    {
       
        public int OrdenId { get; set; }

        public decimal Monto { get; set; }

        public MetodoPago MetodoPago { get; set; }






    }
}
