using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Dtos.UsuarioDto
{
    public class UsuarioResponseDto

    {
        public int Id { get; set; }
        public string Nombre { get; set; } = null!;
        public int Edad { get; set; }
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        
        public string RolUsuario { get; set; } = null!;
        public string EstadoUsuario { get; set; } = null!;





    }
}
