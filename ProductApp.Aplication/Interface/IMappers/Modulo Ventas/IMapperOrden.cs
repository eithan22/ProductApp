using ProductApp.Aplication.Dtos.OrdenDto;
using ProductApp.Domian.Entitis;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Interface.IMappers.Modulo_Ventas
{
    public interface IMapperOrden
    {
        Orden MapTOCreateOrden(CreateOrdenDto dto, int usuarioId);
         OrdenResponseDto MapToOrdenResponseDto(Orden orden);
         void MapToUpdateOrden(UpdateOrdenDto dto, Orden orden);
    }
}
