using FluentValidation;
using ProductApp.Aplication.BusinessValidator.Modulo_Usuarios;
using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto;
using ProductApp.Aplication.Dtos.UsuarioDto;
using ProductApp.Aplication.Helper;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Interface.IMappers.Modulo_Usuarios;
using ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Usuario;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Common.Enums.EnumsUsuario;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Repository;
using System;
using System.ClientModel.Primitives;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Services
{
    public class UsuarioService : IUsuarioService
    {
        public readonly IUsuarioRepository _usuarioRepository;
        public readonly IMapperUsuario _mapperUsuario;
        private readonly IValidator<CreateUsuarioDto> _createValidator;  
        private readonly IValidator<UpdateUsuarioDto> _updateValidator;
        private readonly IValidator<ChangePasswordDto> _changePasswordValidator;
        private readonly IValidator<ResetearPasswordDto> _resetPasswordValidator;
        private readonly IValidator<CambiarRolDto> _cambiarRolValidator;
        private readonly IValitadorBusinessUsuario _validatorBusinessUsuarios;

        public UsuarioService(IUsuarioRepository usuarioRepository, 
            IMapperUsuario mapperUsuario,
            IValidator<CreateUsuarioDto> createvalidator,
            IValidator<UpdateUsuarioDto> updatevalidator,
            IValitadorBusinessUsuario validatorBusinessUsuarios

            )

        { 
            _usuarioRepository = usuarioRepository;
            _mapperUsuario = mapperUsuario;
            _createValidator = createvalidator;
            _updateValidator = updatevalidator;
            _validatorBusinessUsuarios = validatorBusinessUsuarios;
        }


        public async Task<OperationResultD<UsuarioResponseDto>> CreateAsync(CreateUsuarioDto dto)
        {
            //validacion con fluent validation
            var validationResult = await _createValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return OperationResultD<UsuarioResponseDto>.Failure($"Validación fallida: {errors}");
            }

            //validaciones de reglas de negocio

            var businessValidationResult = await _validatorBusinessUsuarios.ValidarCreateUsuarioAsync(dto);

            if (!businessValidationResult.IsSuccess)
            {
                return OperationResultD<UsuarioResponseDto>.Failure(businessValidationResult.Message);
            }

            //crear entidad

            var usuario = _mapperUsuario.MapToEntity(dto);

            usuario.EstablecerPasswordHash(PasswordHelper.Hash(dto.Password));

            await _usuarioRepository.CreateAsync(usuario);

            //respuesta
              var usuarioResponseDto = _mapperUsuario.ToDto(usuario);

            return OperationResultD<UsuarioResponseDto>.Success(usuarioResponseDto , "Usuario Creado Correctamente");

        }


        //solo el usuario puede cambiar su contraseña y debe proporcionar la contraseña actual para validar su identidad
        public async Task<OperationResultD<bool>> CambiarPasswordUsuario(ChangePasswordDto dto)
        {
            //validacion con fluent validation
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
            {
                return OperationResultD<bool>.Failure(validatorBusinessResult.Message);
            }

            

            usuario.EstablecerPasswordHash(PasswordHelper.Hash(dto.PasswordNueva));

            await _usuarioRepository.UpdateAsync(usuario);
            return OperationResultD<bool>.Success(true, "Contraseña actualizada correctamente");
        }



        //solo el admin puede resetear la contraseña de un usuario sin necesidad de conocer la contraseña actual

        public async Task<OperationResultD<bool>> ResetearPassword(ResetearPasswordDto dto)
        {
            //validacion con fluent validation
            var validationResult = await _resetPasswordValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return OperationResultD<bool>.Failure($"Validación fallida: {errors}");
            }



            var usuario = await _usuarioRepository.GetByIdAsync(dto.Id);
            if (usuario == null)
                return OperationResultD<bool>.Failure("Usuario no encontrado");//

            var validatorBusinessResult = await _validatorBusinessUsuarios.ValidarResetearPassword(dto);

            if (!validatorBusinessResult.IsSuccess) {
                return OperationResultD<bool>.Failure(validatorBusinessResult.Message);
            }

            usuario.EstablecerPasswordHash(PasswordHelper.Hash(dto.NuevaContraseña));

            await _usuarioRepository.UpdateAsync(usuario);
            return OperationResultD<bool>.Success(true, "Contraseña reseteada correctamente");
        }


        //solo el admin puede cambiar el rol de un usuario
        public async Task<OperationResultD<bool>> CambiarRol(CambiarRolDto dto)
        {
            //validacion con fluent validation
            var validationResult = await _cambiarRolValidator.ValidateAsync(dto);

            if (!validationResult.IsValid) 
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return OperationResultD<bool>.Failure($"Validación fallida: {errors}");
            };

            var usuario = await _usuarioRepository.GetByIdAsync(dto.Id);
            if(usuario == null)
            {
                return OperationResultD<bool>.Failure("Usuario no encontrado");
            }

            var validatorBusinessResult = await _validatorBusinessUsuarios.ValidarCambiarRol(dto);
            if (!validatorBusinessResult.IsSuccess) {
                return OperationResultD<bool>.Failure(validatorBusinessResult.Message);
            }

            usuario.CambiarRol(dto.NuevoRol);

            await _usuarioRepository.UpdateAsync(usuario);
            return OperationResultD<bool>.Success(true, "Rol actualizado correctamente");
        }


       

        public async Task<OperationResultD<bool>> DeleteAsync(int id)

        {
            if (id <= 0)
            {
                return OperationResultD<bool>.Failure("Id no valido");
            }
            var usuario = await _usuarioRepository.GetByIdAsync(id);

            if (usuario == null)
            {
                return OperationResultD<bool>.Failure("Usuario no encontrado");
            }

            await _usuarioRepository.DeleteAsync(id);

            return OperationResultD<bool>.Success(true, "Usuario Eliminado Correctamente");

        }

        

        public async Task<OperationResultD<bool>> DisableAsync(int id)
        {


            if(id < 0)
            {
                return OperationResultD<bool>.Failure("Id no valido");
            }

            var usuario = await _usuarioRepository.GetByIdAsync(id);
            
            if (usuario == null)
            {
                return OperationResultD<bool>.Failure("Usuario no encontrado");
            }

            var validatorBusinessResult = await _validatorBusinessUsuarios.ValidarDeleteUsuarioAsync(usuario);

            if (!validatorBusinessResult.IsSuccess) {
                return OperationResultD<bool>.Failure(validatorBusinessResult.Message);
            }

            usuario.Desactivar();

            await _usuarioRepository.UpdateAsync(usuario);
            return OperationResultD<bool>.Success(true, "Usuario Deshabilitado Correctamente");



        }



        //No entiedo aun 
        public async Task<OperationResultD<List<UsuarioResponseDto>>> GetAllAsync()
        {
            var usuarios = await _usuarioRepository.GetAllAsync();
            if(usuarios == null || !usuarios.Any())
            {
                return OperationResultD<List<UsuarioResponseDto>>.Failure("No se encontraron usuarios");
            }

            // Mapear con LINQ
            var usuarioResponseDtos = usuarios
                .Select(u => _mapperUsuario.ToDto(u))
                .ToList();

            return OperationResultD<List<UsuarioResponseDto>>.Success(usuarioResponseDtos, "Usuarios obtenidos correctamente");
        }





        public async Task<OperationResultD<UsuarioResponseDto>> GetByIdAsync(int id)
        {
            //bucar en el repositorio
            if (id <= 0)
            {
                return OperationResultD<UsuarioResponseDto>.Failure("Id no valido");
            }

            var usuario = await _usuarioRepository.GetByIdAsync(id);

            //validad exixtencia
          
            if (usuario == null)
            {
                return OperationResultD<UsuarioResponseDto>.Failure("Usuario no encontrado");
            }

            //mapear entidad a dto

            var usuarioResponseDto = _mapperUsuario.ToDto(usuario);

            return OperationResultD<UsuarioResponseDto>.Success(usuarioResponseDto, "Usuario obtenido correctamente");


        }




        public async Task<OperationResultD<UsuarioResponseDto>> UpdateAsync(UpdateUsuarioDto dto)
        {
            

            var usuario = await _usuarioRepository.GetByIdAsync(dto.Id);

            if (usuario == null)
            {
                return OperationResultD<UsuarioResponseDto>.Failure("Usuario no encontrado");
            }

            //validacion con fluent validation
            var validationResult = await _updateValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return OperationResultD<UsuarioResponseDto>.Failure($"Validación fallida: {errors}");
            }


            //validaciones de reglas de negocio
            var businessValidationResult = await _validatorBusinessUsuarios.ValidarUpdateUsuarioAsync(dto);
            if (!businessValidationResult.IsSuccess)
            {
                return OperationResultD<UsuarioResponseDto>.Failure(businessValidationResult.Message);
            }



            //moficicando la entidad existente

            _mapperUsuario.mapUpdate(dto, usuario);

            //guuardar los cambios en el repositorio

            await _usuarioRepository.UpdateAsync(usuario);

            var usuarioResponseDto = _mapperUsuario.ToDto(usuario);

            return OperationResultD<UsuarioResponseDto>.Success(usuarioResponseDto, "Usuario actualizado correctamente");




        }

        
    }
}
