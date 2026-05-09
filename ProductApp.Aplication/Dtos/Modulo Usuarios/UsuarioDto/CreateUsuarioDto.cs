using Org.BouncyCastle.Asn1.X509;
using ProductApp.Domian.Common.Enums.EnumsUsuario;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Dtos.UsuarioDto
{
    public class CreateUsuarioDto
    {
        public string Nombre { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;

        public int Edad { get; set; }
        public string UserName { get; set; } = string.Empty;

        public RolUsuario RolUsuario { get; set; }





    }
}
    