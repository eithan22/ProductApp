using ProductApp.Aplication.Common;
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

        Task<OperationResultD<bool>> ResetearPassword(ResetearPasswordDto dto, int usuarioSolicitanteId);

        Task<OperationResultD<bool>> CambiarRol(CambiarRolDto dto, int usuarioSolicitanteId);

        // Sobrecarga propia de Usuario (la firma de IBaseServices no lleva solicitante):
        // desactivar una cuenta es una acción administrativa sensible y tiene que quedar
        // registrada con el id de quien la ejecutó, igual que CambiarRol y ResetearPassword.
        Task<OperationResultD<bool>> DisableAsync(int id, int usuarioSolicitanteId);

        Task<OperationResultD<PagedResult<UsuarioResponseDto>>> GetAllAsync(bool incluirInactivos, int pageNumber = 1, int pageSize = PaginacionDefaults.PageSizeDefault);

        Task<OperationResultD<bool>> EnableUsuario(int id);

        Task<OperationResultD<UsuarioResponseDto>> ObtenerMiPerfilAsync(int usuarioId);

        Task<OperationResultD<UsuarioResponseDto>> ActualizarMiPerfilAsync(int usuarioId, ActualizarMiPerfilDto dto);

        // El id sale del token, igual que en MiPerfil: nadie acepta en nombre de otro.
        Task<OperationResultD<bool>> RegistrarAceptacionDocumentosLegalesAsync(int usuarioId, AceptarDocumentosLegalesDto dto);
    }
}
