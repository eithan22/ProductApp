using ProductApp.Aplication.Dtos.OrdenDto;
using ProductApp.Aplication.Interface.IMappers.Modulo_Ventas;
using ProductApp.Domian.Common.Enums.EnumsOrden;
using ProductApp.Domian.Entitis;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Mappers.Modulo_Ventas
{
    public class OrdenMapper : IMapperOrden
    {
        public Orden MapTOCreateOrden(CreateOrdenDto dto, int usuarioid)
        {
            return new Orden
            (
                dto.ClienteId,
                usuarioid
            );
            
               
    
            
        }

       

        public OrdenResponseDto MapToOrdenResponseDto(Orden orden)
        {
            var response = new OrdenResponseDto
            {
                Id = orden.Id,
                NombreCliente = orden.Cliente.Nombre,   
                Fecha = orden.Fecha,
                Estado = orden.Estado.ToString(),
                Total = orden.Total
            };
            return response;
        }

    }
}
