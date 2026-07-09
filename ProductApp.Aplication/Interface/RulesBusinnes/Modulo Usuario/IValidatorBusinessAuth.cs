using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto.AuthDto;
using ProductApp.Aplication.Result.OperationResult;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Usuario
{
    public interface IValidatorBusinessAuth
    {
         Task<OperationResult> ValidarLoginAsync(LoginDto dto);
    }
}
