using ProductApp.Aplication.Dtos.CategoriaDto;
using ProductApp.Aplication.Interface.Servicios.BaseServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Interface
{
    public interface ICategoriaServices : IBaseServices<CategoriaResponseDto, CreateCategoriaDto, UpdateCategoriaDto>
    {
    }
}
