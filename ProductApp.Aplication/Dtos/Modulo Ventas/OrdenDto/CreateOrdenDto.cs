using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Dtos.OrdenDto
{
    public class CreateOrdenDto
    {
        public int Id { get; set; }   
        public int ClienteId { get; set; }

        public decimal Total { get; set; } = 0;


    }
}
