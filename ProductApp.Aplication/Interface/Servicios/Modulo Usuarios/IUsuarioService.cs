using ProductApp.Aplication.Dtos.UsuarioDto;
using ProductApp.Aplication.Interface.Servicios.BaseServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Interface
{
    public interface IUsuarioService : IBaseServices<UsuarioResponseDto, CreateUsuarioDto, UpdateUsuarioDto>
    {
      


    }
}
