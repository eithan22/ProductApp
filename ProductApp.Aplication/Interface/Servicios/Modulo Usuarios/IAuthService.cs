using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto.AuthDto;
using ProductApp.Aplication.Dtos.UsuarioDto;
using ProductApp.Aplication.Result.OperationResult;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Interface.Servicios.Modulo_Usuarios
{
    public interface IAuthService
    {
        Task<OperationResultD<AuthResponseDto>>Login(LoginDto dto);
    }
}
