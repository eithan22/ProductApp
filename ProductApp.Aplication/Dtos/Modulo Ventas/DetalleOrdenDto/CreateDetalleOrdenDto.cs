using ProductApp.Domian.Entitis;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Dtos.Modulo_Ventas.DetalleOrdenDto
{
    public class CreateDetalleOrdenDto
    {

        public int ProductId { get; set; }
        
        public int Cantidad { get; set; }

        

        public int OrdenId { get; set; }
       
    }
}
