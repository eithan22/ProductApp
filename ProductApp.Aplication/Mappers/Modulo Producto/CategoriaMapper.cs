using ProductApp.Aplication.Dtos.CategoriaDto;
using ProductApp.Aplication.Interface.IMappers.Modulos_Productos;
using ProductApp.Domian.Entitis;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Mappers.Modulo_Producto
{
    public class CategoriaMapper : IMapperCategoria
    {
        public CategoriaResponseDto MapToCategoriaResponseDto(Categoria categoria)
        {
            var categoriaResponseDto = new CategoriaResponseDto
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                Descripcion = categoria.Descripcion,
                Estado = categoria.EstaEliminado ? "Inactivo" : "Activo"

            };
            return categoriaResponseDto;
        }

        public Categoria MapToCreateCategoria(CreateCategoriaDto dto)
        {
            return new Categoria(
                 dto.Nombre,
                 dto.Descripcion
             );
        }

        public void MapToUpdateCategoria(UpdateCategoriaDto dto, Categoria categoria)
        {
           
            categoria.CambiarYvalidarNombre(dto.Nombre);
            categoria.CambiarYvalidarDescripcion(dto.Descripcion);
        }
    }
}
