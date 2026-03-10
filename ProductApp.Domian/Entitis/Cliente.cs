using ProductApp.Domian.Common.Base;
using ProductApp.Domian.Common.Enums.EnumsCliente;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Domian.Entitis
{
    public class Cliente : BaseEntity
    {
        
        public string Nombre { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;

        public string Direccion { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;

        public EstadoCliente Estado { get; set; }

        public List<Orden> Ordenes { get; set; } = new List<Orden>();
       

    }
}
