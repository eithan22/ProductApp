using ProductApp.Domian.Common.Enums.EnumsCliente;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Dtos.ClienteDto
{
    public class ClienteResponseDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty; 

        public string Email { get; set; }  = string.Empty;
        public string Telefono { get; set; } = string.Empty;

        public string Cedula { get; set; } = string.Empty;

        public string Direccion { get; set; } = string.Empty;

        public string Estado { get; set; } = string.Empty; //para poder mapearlo a string y mostrar si esta activo o desactivado sin el enum

        // Calculado en el mapper a partir del dominio, no persistido. Le dice a la Web
        // que este cliente es el "Consumidor Final" del sistema, para pintar el candado
        // en el listado y preseleccionarlo al crear una orden.
        public bool EsReservado { get; set; }

    }
}
