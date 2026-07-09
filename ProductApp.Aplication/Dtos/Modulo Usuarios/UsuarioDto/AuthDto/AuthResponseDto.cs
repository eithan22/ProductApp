using ProductApp.Aplication.Dtos.UsuarioDto;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto.AuthDto
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public bool DebeCambiarPassword { get; set; }
        public UsuarioResponseDto Usuario { get; set; } = null!;
    }
}
