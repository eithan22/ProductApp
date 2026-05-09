using ProductApp.Domian.Common.Enums.EnumsUsuario;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto
{
    public class CambiarRolDto
    {
        public int Id { get; set; }
        public RolUsuario NuevoRol { get; set; }
    }
}
