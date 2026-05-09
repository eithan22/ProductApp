using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto.AuthDto;
using ProductApp.Aplication.Dtos.UsuarioDto;
using ProductApp.Aplication.Interface.IMappers.Modulo_Usuarios;
using ProductApp.Domian.Common.Enums.EnumsUsuario;
using ProductApp.Domian.Entitis;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Mappers
{
    public class UsuarioMapper : IMapperUsuario
    {

        //transformar un CreateUsuarioDto a un Usuario
        public Usuario MapToEntity(CreateUsuarioDto dto)
        {
            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Edad = dto.Edad,
                Username = dto.UserName,
                Email = dto.Email,
                RolUsuario = dto.RolUsuario,
                EstadoUsuario = EstadoUsuario.Activo


            };
            return usuario;
        }

        //transformar un Usuario a un UsuarioResponseDto
        public UsuarioResponseDto ToDto(Usuario usuario)
        {
            var usuarioResponseDto = new UsuarioResponseDto
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre,
                Email = usuario.Email,
                UserName = usuario.Username,
                Edad = usuario.Edad,
                RolUsuario = usuario.RolUsuario.ToString(),
                EstadoUsuario = usuario.EstadoUsuario.ToString()
            };

            return usuarioResponseDto;
        }

        public void mapUpdate(UpdateUsuarioDto dto, Usuario usuario)
        {
            usuario.Nombre = dto.Nombre;
            usuario.Email = dto.Email;
            usuario.Username = dto.UserName;
            usuario.Edad = dto.Edad;
            usuario.RolUsuario = dto.RolUsuario;
            
           

        }

        //transformar un RegisteDto a un Usuario
        public Usuario MapFromRegisterDto(RegisteDto dto)
        {
            var usuario = new Usuario
            {
                Nombre = dto.Nombre,
                Edad = dto.Edad,
                Username = dto.UserName,
                Email = dto.Email,
                RolUsuario = RolUsuario.Vendedor, 
                EstadoUsuario = EstadoUsuario.Activo
            };
            return usuario;
        }
    }
}
