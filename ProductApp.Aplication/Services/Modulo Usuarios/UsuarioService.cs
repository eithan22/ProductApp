using FluentValidation;
using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto;
using ProductApp.Aplication.Dtos.UsuarioDto;
using ProductApp.Aplication.Helper;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Interface.IMappers.Modulo_Usuarios;
using ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Usuario;
using ProductApp.Aplication.Result.OperationResult;
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
        private readonly IValidatorBusinessUsuario _validatorBusinessUsuarios;

        public UsuarioService(
            IUsuarioRepository usuarioRepository,
            IMapperUsuario mapperUsuario,
            IValidator<CreateUsuarioDto> createValidator,
            IValidator<UpdateUsuarioDto> updateValidator,
            IValidatorBusinessUsuario validatorBusinessUsuarios,
            IValidator<ChangePasswordDto> changePasswordValidator,
            IValidator<ResetearPasswordDto> resetPasswordValidator,
            IValidator<CambiarRolDto> cambiarRolValidator)
        {
            _usuarioRepository = usuarioRepository;
            _mapperUsuario = mapperUsuario;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _validatorBusinessUsuarios = validatorBusinessUsuarios;
            _changePasswordValidator = changePasswordValidator;
            _resetPasswordValidator = resetPasswordValidator;
            _cambiarRolValidator = cambiarRolValidator;
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
            await _usuarioRepository.UpdateAsync(usuario);

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

            usuario.CambiarRol(dto.NuevoRol);
            await _usuarioRepository.UpdateAsync(usuario);

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

        public async Task<OperationResultD<List<UsuarioResponseDto>>> GetAllAsync()
        {
            var usuarios = await _usuarioRepository.GetAllAsync();
            if (usuarios == null || !usuarios.Any())
                return OperationResultD<List<UsuarioResponseDto>>.Failure("No se encontraron usuarios");

            var usuarioResponseDtos = usuarios.Select(u => _mapperUsuario.ToDto(u)).ToList();
            return OperationResultD<List<UsuarioResponseDto>>.Success(usuarioResponseDtos, "Usuarios obtenidos correctamente");
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
            var usuario = await _usuarioRepository.GetByIdAsync(dto.Id);
            if (usuario == null)
                return OperationResultD<UsuarioResponseDto>.Failure("Usuario no encontrado");

            var validationResult = await _updateValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return OperationResultD<UsuarioResponseDto>.Failure($"Validación fallida: {errors}");
            }

            var businessValidationResult = await _validatorBusinessUsuarios.ValidarUpdateUsuarioAsync(dto);
            if (!businessValidationResult.IsSuccess)
                return OperationResultD<UsuarioResponseDto>.Failure(businessValidationResult.Message);

            _mapperUsuario.mapUpdate(dto, usuario);
            await _usuarioRepository.UpdateAsync(usuario);

            var usuarioResponseDto = _mapperUsuario.ToDto(usuario);
            return OperationResultD<UsuarioResponseDto>.Success(usuarioResponseDto, "Usuario actualizado correctamente");
        }
    }
}
