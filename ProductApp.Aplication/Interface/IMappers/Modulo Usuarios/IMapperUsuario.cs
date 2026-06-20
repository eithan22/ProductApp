using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto.AuthDto;
using ProductApp.Aplication.Dtos.UsuarioDto;
using ProductApp.Domian.Entitis;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Interface.IMappers.Modulo_Usuarios
{
    public interface IMapperUsuario
    {
       UsuarioResponseDto ToDto(Usuario usuario);

        Usuario MapToEntity(CreateUsuarioDto dto);


        void mapUpdate(UpdateUsuarioDto dto, Usuario usuario);

        Usuario MapFromRegisterDto(RegisterDto dto);






    }
}
