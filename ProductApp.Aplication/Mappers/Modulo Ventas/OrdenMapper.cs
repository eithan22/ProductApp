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
        public Orden MapTOCreateOrden(CreateOrdenDto dto)
        {
            return new Orden
            {

                ClienteId = dto.ClienteId,
                Total = 0
            };
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

        public void MapToUpdateOrden(UpdateOrdenDto dto, Orden orden)
        {
            throw new NotImplementedException();
        }
    }
}
