using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto;
using ProductApp.Aplication.Dtos.UsuarioDto;
using ProductApp.Aplication.Interface.Servicios.BaseServices;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Common.Enums.EnumsUsuario;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Interface
{
    public interface IUsuarioService : IBaseServices<UsuarioResponseDto, CreateUsuarioDto, UpdateUsuarioDto>
    {
        Task<OperationResultD<bool>> CambiarPasswordUsuario( ChangePasswordDto dto);

        Task<OperationResultD<bool>> ResetearPassword(ResetearPasswordDto dto);

        Task<OperationResultD<bool>> CambiarRol(CambiarRolDto dto);


    }
}
