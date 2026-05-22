using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Dtos.Modulo_Ventas.DetalleOrdenDto
{
    public class OrdenDetalleResponseDto
    {
        public int ID { get; set; }

        public string Producto { get; set; }= null!;

        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal { get; set; }

        public int OrdenId { get; set; }
    }
}
