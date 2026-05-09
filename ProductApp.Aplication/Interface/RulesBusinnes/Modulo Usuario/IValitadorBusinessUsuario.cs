using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto;
using ProductApp.Aplication.Dtos.UsuarioDto;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Entitis;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Usuario
{
    public interface IValitadorBusinessUsuario
    {
         Task<OperationResult> ValidarCreateUsuarioAsync(CreateUsuarioDto dto);
        Task<OperationResult> ValidarUpdateUsuarioAsync(UpdateUsuarioDto dto);
         Task<OperationResult> ValidarDeleteUsuarioAsync(Usuario usuario);

        Task<OperationResult> ValidarCambiarPasswordUsuario(ChangePasswordDto dto, Usuario usuario);
        Task<OperationResult> ValidarResetearPassword (ResetearPasswordDto dto);

        Task<OperationResult> ValidarCambiarRol (CambiarRolDto dto);

    }
}
