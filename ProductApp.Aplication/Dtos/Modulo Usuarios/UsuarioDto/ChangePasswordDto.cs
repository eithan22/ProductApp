using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto
{
    public class ChangePasswordDto
    {
        public int Id { get; set; }
        public string PasswordActual { get; set; } = null!;
        public string PasswordNueva { get; set; } = null!;
    }
}
