using ProductApp.Aplication.Dtos.UsuarioDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Interface.BaseServices;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Services
{
    public class UsuarioService : IUsuarioService
    {
        public readonly IUsuarioRepository usuarioRepository;

        public UsuarioService(IUsuarioRepository usuarioRepository)
        { 
            this.usuarioRepository = usuarioRepository;
        }


        public async Task<UsuarioResponseDto> CreateAsync(CreateUsuarioDto dto)
        {

            //validaciones
            if(string.IsNullOrEmpty(dto.Nombre))
            {
                throw new Exception("El nombre es requerido");
            }

            
            

            //crear entidad
            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Email = dto.Email,
                Password = dto.Password

            };
            
            await usuarioRepository.CreateAsync(usuario);

                var usuarioResponseDto = new UsuarioResponseDto
                {
                    Id = usuario.Id,
                    Nombre = usuario.Nombre,
                    Email = usuario.Email,
                    RolUsuario = usuario.RolUsuario.ToString(),
                    EstadoUsuario = usuario.EstadoUsuario.ToString()
                };

            return usuarioResponseDto;

        }

        public async Task DeleteAsync(int id)

        {
            if (id <= 0)
            {
                throw new Exception("Id no valido");
            }
            var usuario = await usuarioRepository.GetByIdAsync(id);

            if (usuario == null)
            {
                throw new Exception("Usuario no encontrado");
            }

            await usuarioRepository.DeleteAsync(id);

        }

        public async Task DisableAsync(int id)
        {

            if(id < 0)
            {
                throw new Exception("Id no valido");
            }

            var usuario = await usuarioRepository.GetByIdAsync(id);
            
            if (usuario == null)
            {
                throw new Exception("Usuario no encontrado");
            }
            await usuarioRepository.DisebleAsync(id);


        }



        //No entiedo aun 
        public async Task<List<UsuarioResponseDto>> GetAllAsync()
        {
            var usuarios = await usuarioRepository.GetAllAsync();

            // Mapear con LINQ
            var usuarioResponseDtos = usuarios
                .Select(u => new UsuarioResponseDto
                {
                    Id = u.Id,
                    Nombre = u.Nombre,
                    Email = u.Email,
                    RolUsuario = u.RolUsuario.ToString(),
                    EstadoUsuario = u.EstadoUsuario.ToString()
                })
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

            var usuario = await usuarioRepository.GetByIdAsync(id);

            //validad exixtencia
          
            if (usuario == null)
            {
                throw new Exception("Usuario no encontrado");
            }

            //mapear entidad a dto

            var usuarioResponseDto = new UsuarioResponseDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                RolUsuario = usuario.RolUsuario.ToString(),
                EstadoUsuario = usuario.EstadoUsuario.ToString()
            };

            return usuarioResponseDto;


        }

        public async Task<UsuarioResponseDto> UpdateAsync(UpdateUsuarioDto dto)
        {
            var usuario = await usuarioRepository.GetByIdAsync(dto.Id);

            if (usuario == null)
            {
                throw new Exception("Usuario no encontrado");
            }

            //moficicando la entidad existente
           
            usuario.Nombre = dto.Nombre;
            usuario.Email = dto.Email;

            //guuardar los cambios en el repositorio

            await usuarioRepository.UpdateAsync(usuario);

            var usuarioResponseDto = new UsuarioResponseDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                RolUsuario = usuario.RolUsuario.ToString(),
                EstadoUsuario = usuario.EstadoUsuario.ToString()
            };

            return usuarioResponseDto;




        }

        
    }
}
