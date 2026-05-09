using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto
{
    public class ResetearPasswordModel
    {
        public int Id { get; set; }
        public string NuevaContraseña { get; set; } = null!;
            
    }
}
