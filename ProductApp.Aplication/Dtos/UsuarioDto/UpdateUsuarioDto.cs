using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Dtos.UsuarioDto
{
    public class UpdateUsuarioDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public string Email { get; set; } = null!;
        //public string Password { get; set; } = null!;
    }
}
