using ProductApp.Aplication.Dtos.CategoriaDto;
using ProductApp.Aplication.Dtos.ClienteDto;
using ProductApp.Domian.Entitis;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Interface.IMappers.Modulos_Productos
{
    public interface IMapperCategoria
    {
        CategoriaResponseDto MapToCategoriaResponseDto(Categoria categoria);
        Categoria MapToCreateCategoria(CreateCategoriaDto dto);
        void MapToUpdateCategoria(UpdateCategoriaDto dto, Categoria categoria);
    }
}
