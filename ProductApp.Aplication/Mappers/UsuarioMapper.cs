using ProductApp.Aplication.Dtos.UsuarioDto;
using ProductApp.Aplication.Interface.IMappers.Modulo_Usuarios;
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
                Email = dto.Email,
                Password = dto.Password
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
                RolUsuario = usuario.RolUsuario.ToString(),
                EstadoUsuario = usuario.EstadoUsuario.ToString()
            };

            return usuarioResponseDto;
        }

        public void mapUpdate(UpdateUsuarioDto dto, Usuario usuario)
        {
            usuario.Nombre = dto.Nombre;
            usuario.Email = dto.Email;
            



        }
    }
}
