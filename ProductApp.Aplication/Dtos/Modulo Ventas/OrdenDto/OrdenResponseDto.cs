using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ProductApp.Aplication.Dtos.OrdenDto
{
    public class OrdenResponseDto
    {
        public int Id { get; set; }
        public string NombreCliente { get; set; } = null!;
        public decimal Total { get; set; } = 0;

        public string Estado { get; set; } = null!;

        public DateTime Fecha { get; set; }

    }
}
