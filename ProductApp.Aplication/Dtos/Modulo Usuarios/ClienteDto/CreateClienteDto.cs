using ProductApp.Domian.Common.Enums.EnumsCliente;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Dtos.ClienteDto
{
    public class CreateClienteDto 
    {
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;

        public string Cedula { get; set; } = string.Empty;

        public string Direccion { get; set; } = string.Empty;

       




        }
}
