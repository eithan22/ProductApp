using FluentValidation;
using Microsoft.Extensions.Logging;
using ProductApp.Aplication.Common;
using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto;
using ProductApp.Aplication.Dtos.UsuarioDto;
using ProductApp.Aplication.Helper;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Interface.IMappers.Modulo_Usuarios;
using ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Usuario;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Common.Enums.EnumsUsuario;
using ProductApp.Domian.Interfaces;

namespace ProductApp.Aplication.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IMapperUsuario _mapperUsuario;
        private readonly IValidator<CreateUsuarioDto> _createValidator;
        private readonly IValidator<UpdateUsuarioDto> _updateValidator;
        private readonly IValidator<ChangePasswordDto> _changePasswordValidator;
        private readonly IValidator<ResetearPasswordDto> _resetPasswordValidator;
        private readonly IValidator<CambiarRolDto> _cambiarRolValidator;
        private readonly IValidator<ActualizarMiPerfilDto> _actualizarMiPerfilValidator;
        private readonly IValidatorBusinessUsuario _validatorBusinessUsuarios;
        private readonly ILogger<UsuarioService> _logger;

        public UsuarioService(
            IUsuarioRepository usuarioRepository,
            IMapperUsuario mapperUsuario,
            IValidator<CreateUsuarioDto> createValidator,
            IValidator<UpdateUsuarioDto> updateValidator,
            IValidatorBusinessUsuario validatorBusinessUsuarios,
            IValidator<ChangePasswordDto> changePasswordValidator,
            IValidator<ResetearPasswordDto> resetPasswordValidator,
            IValidator<CambiarRolDto> cambiarRolValidator,
            IValidator<ActualizarMiPerfilDto> actualizarMiPerfilValidator,
            ILogger<UsuarioService> logger)
        {
            _usuarioRepository = usuarioRepository;
            _mapperUsuario = mapperUsuario;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _validatorBusinessUsuarios = validatorBusinessUsuarios;
            _changePasswordValidator = changePasswordValidator;
            _resetPasswordValidator = resetPasswordValidator;
            _cambiarRolValidator = cambiarRolValidator;
            _actualizarMiPerfilValidator = actualizarMiPerfilValidator;
            _logger = logger;
        }

        public async Task<OperationResultD<UsuarioResponseDto>> CreateAsync(CreateUsuarioDto dto)
        {
            var validationResult = await _createValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return OperationResultD<UsuarioResponseDto>.Failure($"Validación fallida: {errors}");
            }

            var businessValidationResult = await _validatorBusinessUsuarios.ValidarCreateUsuarioAsync(dto);
            if (!businessValidationResult.IsSuccess)
                return OperationResultD<UsuarioResponseDto>.Failure(businessValidationResult.Message);

            var usuario = _mapperUsuario.MapToEntity(dto);
            usuario.EstablecerFechaNacimiento(dto.FechaNacimiento);
            usuario.EstablecerPasswordHash(PasswordHelper.Hash(dto.Password));
            usuario.MarcarPasswordComoTemporal();

            await _usuarioRepository.CreateAsync(usuario);

            var usuarioResponseDto = _mapperUsuario.ToDto(usuario);
            return OperationResultD<UsuarioResponseDto>.Success(usuarioResponseDto, "Usuario Creado Correctamente");
        }

        public async Task<OperationResultD<bool>> CambiarPasswordUsuario(ChangePasswordDto dto)
        {
            var validationResult = await _changePasswordValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return OperationResultD<bool>.Failure($"Validación fallida: {errors}");
            }

            var usuario = await _usuarioRepository.GetByIdAsync(dto.Id);
            if (usuario == null)
                return OperationResultD<bool>.Failure("Usuario no encontrado");

            var validatorBusinessResult = await _validatorBusinessUsuarios.ValidarCambiarPasswordUsuario(dto, usuario);
            if (!validatorBusinessResult.IsSuccess)
                return OperationResultD<bool>.Failure(validatorBusinessResult.Message);

            usuario.EstablecerPasswordHash(PasswordHelper.Hash(dto.PasswordNueva));
            usuario.ConfirmarCambioPassword();
            await _usuarioRepository.UpdateAsync(usuario);

            return OperationResultD<bool>.Success(true, "Contraseña actualizada correctamente");
        }

        public async Task<OperationResultD<bool>> ResetearPassword(ResetearPasswordDto dto)
        {
            var validationResult = await _resetPasswordValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return OperationResultD<bool>.Failure($"Validación fallida: {errors}");
            }

            var usuario = await _usuarioRepository.GetByIdAsync(dto.Id);
            if (usuario == null)
                return OperationResultD<bool>.Failure("Usuario no encontrado");

            var validatorBusinessResult = await _validatorBusinessUsuarios.ValidarResetearPassword(dto);
            if (!validatorBusinessResult.IsSuccess)
                return OperationResultD<bool>.Failure(validatorBusinessResult.Message);

            usuario.EstablecerPasswordHash(PasswordHelper.Hash(dto.NuevaContraseña));
            usuario.MarcarPasswordComoTemporal();
            await _usuarioRepository.UpdateAsync(usuario);

            _logger.LogInformation("Contraseña reseteada para el usuario {UsuarioId}", usuario.Id);

            return OperationResultD<bool>.Success(true, "Contraseña reseteada correctamente");
        }

        public async Task<OperationResultD<bool>> CambiarRol(CambiarRolDto dto)
        {
            var validationResult = await _cambiarRolValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return OperationResultD<bool>.Failure($"Validación fallida: {errors}");
            }

            var usuario = await _usuarioRepository.GetByIdAsync(dto.Id);
            if (usuario == null)
                return OperationResultD<bool>.Failure("Usuario no encontrado");

            var validatorBusinessResult = await _validatorBusinessUsuarios.ValidarCambiarRol(dto);
            if (!validatorBusinessResult.IsSuccess)
                return OperationResultD<bool>.Failure(validatorBusinessResult.Message);

            var nuevoRol = Enum.Parse<RolUsuario>(dto.NuevoRol, true);
            usuario.CambiarRol(nuevoRol);
            await _usuarioRepository.UpdateAsync(usuario);

            _logger.LogInformation("Rol actualizado para el usuario {UsuarioId}: nuevo rol {NuevoRol}", usuario.Id, nuevoRol);

            return OperationResultD<bool>.Success(true, "Rol actualizado correctamente");
        }

        public async Task<OperationResultD<bool>> DeleteAsync(int id)
        {
            if (id <= 0)
                return OperationResultD<bool>.Failure("Id no valido");

            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario == null)
                return OperationResultD<bool>.Failure("Usuario no encontrado");

            await _usuarioRepository.DeleteAsync(id);
            return OperationResultD<bool>.Success(true, "Usuario Eliminado Correctamente");
        }

        public async Task<OperationResultD<bool>> DisableAsync(int id)
        {
            if (id <= 0)
                return OperationResultD<bool>.Failure("Id no valido");

            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario == null)
                return OperationResultD<bool>.Failure("Usuario no encontrado");

            var validatorBusinessResult = await _validatorBusinessUsuarios.ValidarDeleteUsuarioAsync(usuario);
            if (!validatorBusinessResult.IsSuccess)
                return OperationResultD<bool>.Failure(validatorBusinessResult.Message);

            usuario.Desactivar();
            await _usuarioRepository.UpdateAsync(usuario);

            return OperationResultD<bool>.Success(true, "Usuario Deshabilitado Correctamente");
        }

        public async Task<OperationResultD<bool>> EnableUsuario(int id)
        {
            if (id <= 0)
                return OperationResultD<bool>.Failure("Id no valido");

            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario == null)
                return OperationResultD<bool>.Failure("Usuario no encontrado");

            usuario.Activar();
            await _usuarioRepository.UpdateAsync(usuario);

            return OperationResultD<bool>.Success(true, "Usuario activado correctamente");
        }

        public Task<OperationResultD<PagedResult<UsuarioResponseDto>>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
            => GetAllAsync(incluirInactivos: false, pageNumber, pageSize);

        public async Task<OperationResultD<PagedResult<UsuarioResponseDto>>> GetAllAsync(bool incluirInactivos, int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1)
                return OperationResultD<PagedResult<UsuarioResponseDto>>.Failure("pageNumber debe ser mayor o igual a 1");

            if (pageSize < 1 || pageSize > 100)
                return OperationResultD<PagedResult<UsuarioResponseDto>>.Failure("pageSize debe estar entre 1 y 100");

            var (usuarios, totalCount) = await _usuarioRepository.GetAllUsuariosAsync(incluirInactivos, pageNumber, pageSize);

            var usuarioResponseDtos = usuarios.Select(u => _mapperUsuario.ToDto(u)).ToList();

            var pagedResult = new PagedResult<UsuarioResponseDto>
            {
                Items = usuarioResponseDtos,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return OperationResultD<PagedResult<UsuarioResponseDto>>.Success(pagedResult, "Usuarios obtenidos correctamente");
        }

        public async Task<OperationResultD<UsuarioResponseDto>> GetByIdAsync(int id)
        {
            if (id <= 0)
                return OperationResultD<UsuarioResponseDto>.Failure("Id no valido");

            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario == null)
                return OperationResultD<UsuarioResponseDto>.Failure("Usuario no encontrado");

            var usuarioResponseDto = _mapperUsuario.ToDto(usuario);
            return OperationResultD<UsuarioResponseDto>.Success(usuarioResponseDto, "Usuario obtenido correctamente");
        }

        public async Task<OperationResultD<UsuarioResponseDto>> UpdateAsync(UpdateUsuarioDto dto)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return OperationResultD<UsuarioResponseDto>.Failure($"Validación fallida: {errors}");
            }

            var usuario = await _usuarioRepository.GetByIdAsync(dto.Id);
            if (usuario == null)
                return OperationResultD<UsuarioResponseDto>.Failure("Usuario no encontrado");

            var businessValidationResult = await _validatorBusinessUsuarios.ValidarUpdateUsuarioAsync(dto);
            if (!businessValidationResult.IsSuccess)
                return OperationResultD<UsuarioResponseDto>.Failure(businessValidationResult.Message);

            _mapperUsuario.MapUpdate(dto, usuario);
            await _usuarioRepository.UpdateAsync(usuario);

            var usuarioResponseDto = _mapperUsuario.ToDto(usuario);
            return OperationResultD<UsuarioResponseDto>.Success(usuarioResponseDto, "Usuario actualizado correctamente");
        }

        public async Task<OperationResultD<UsuarioResponseDto>> ObtenerMiPerfilAsync(int usuarioId)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
            if (usuario == null)
                return OperationResultD<UsuarioResponseDto>.Failure("Usuario no encontrado");

            var usuarioResponseDto = _mapperUsuario.ToDto(usuario);
            return OperationResultD<UsuarioResponseDto>.Success(usuarioResponseDto, "Perfil obtenido correctamente");
        }

        public async Task<OperationResultD<UsuarioResponseDto>> ActualizarMiPerfilAsync(int usuarioId, ActualizarMiPerfilDto dto)
        {
            var validationResult = await _actualizarMiPerfilValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return OperationResultD<UsuarioResponseDto>.Failure($"Validación fallida: {errors}");
            }

            var usuario = await _usuarioRepository.GetByIdAsync(usuarioId);
            if (usuario == null)
                return OperationResultD<UsuarioResponseDto>.Failure("Usuario no encontrado");

            usuario.CambiarNombre(dto.Nombre);
            usuario.CambiarEmail(dto.Email);
            usuario.EstablecerFechaNacimiento(dto.FechaNacimiento);
            await _usuarioRepository.UpdateAsync(usuario);

            var usuarioResponseDto = _mapperUsuario.ToDto(usuario);
            return OperationResultD<UsuarioResponseDto>.Success(usuarioResponseDto, "Perfil actualizado correctamente");
        }
    }
}
