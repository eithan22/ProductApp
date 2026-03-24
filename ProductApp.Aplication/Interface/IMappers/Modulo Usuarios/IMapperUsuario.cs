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


       public void mapUpdate(UpdateUsuarioDto dto, Usuario usuario);






    }
}
