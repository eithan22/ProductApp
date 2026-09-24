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
                // Con '?.' por la misma razón que ProductoMapper: todas las consultas de
                // Orden hacen Include(Cliente), pero una orden recién construida por
                // MapTOCreateOrden todavía no lo tiene cargado y el mapper no debe reventar
                // por eso. Se responde sin el nombre, no con un 500.
                NombreCliente = orden.Cliente?.Nombre ?? string.Empty,
                Fecha = orden.Fecha,
                Estado = orden.Estado.ToString(),
                Total = orden.Total
            };
            return response;
        }

    }
}
