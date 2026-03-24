using FluentValidation;
using ProductApp.Aplication.Dtos.UsuarioDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Interface.IMappers.Modulo_Usuarios;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Repository;
using System;
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

        public UsuarioService(IUsuarioRepository usuarioRepository, 
            IMapperUsuario mapperUsuario,
            IValidator<CreateUsuarioDto> createvalidator,
            IValidator<UpdateUsuarioDto> updatevalidator)

        { 
            _usuarioRepository = usuarioRepository;
            _mapperUsuario = mapperUsuario;
            _createValidator = createvalidator;
            _updateValidator = updatevalidator;
        }


        public async Task<UsuarioResponseDto> CreateAsync(CreateUsuarioDto dto)
        {

           
            
            //validacion con fluent validation
            var validationResult = await _createValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new Exception($"Validación fallida: {errors}");
            }

            //crear entidad

            var usuario = _mapperUsuario.MapToEntity(dto);

            await _usuarioRepository.CreateAsync(usuario);

            //respuesta
              var usuarioResponseDto = _mapperUsuario.ToDto(usuario);

            return usuarioResponseDto;

        }

        public async Task DeleteAsync(int id)

        {
            if (id <= 0)
            {
                throw new Exception("Id no valido");
            }
            var usuario = await _usuarioRepository.GetByIdAsync(id);

            if (usuario == null)
            {
                throw new Exception("Usuario no encontrado");
            }

            await _usuarioRepository.DeleteAsync(id);

        }

        public async Task DisableAsync(int id)
        {

            if(id < 0)
            {
                throw new Exception("Id no valido");
            }

            var usuario = await _usuarioRepository.GetByIdAsync(id);
            
            if (usuario == null)
            {
                throw new Exception("Usuario no encontrado");
            }
           // await _usuarioRepository.DisebleAsync(id);


        }



        //No entiedo aun 
        public async Task<List<UsuarioResponseDto>> GetAllAsync()
        {
            var usuarios = await _usuarioRepository.GetAllAsync();

            // Mapear con LINQ
            var usuarioResponseDtos = usuarios
                .Select(u => _mapperUsuario.ToDto(u))
                .ToList();

            return usuarioResponseDtos;
        }


        public async Task<UsuarioResponseDto> GetByIdAsync(int id)
        {
            //bucar en el repositorio
            if (id <= 0)
            {
                throw new Exception("Id no valido");
            }

            var usuario = await _usuarioRepository.GetByIdAsync(id);

            //validad exixtencia
          
            if (usuario == null)
            {
                throw new Exception("Usuario no encontrado");
            }

            //mapear entidad a dto

            var usuarioResponseDto = _mapperUsuario.ToDto(usuario);

            return usuarioResponseDto;


        }

        public async Task<UsuarioResponseDto> UpdateAsync(UpdateUsuarioDto dto)
        {
            //validaciones de entrada del dto con fullent validation

            var validationResult = await _updateValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                throw new Exception($"Validación fallida: {errors}");
            }


            var usuario = await _usuarioRepository.GetByIdAsync(dto.Id);

            if (usuario == null)
            {
                throw new Exception("Usuario no encontrado");
            }

            //moficicando la entidad existente

            _mapperUsuario.mapUpdate(dto, usuario);

            //guuardar los cambios en el repositorio

            await _usuarioRepository.UpdateAsync(usuario);

            var usuarioResponseDto = _mapperUsuario.ToDto(usuario);

            return usuarioResponseDto;




        }

        
    }
}
