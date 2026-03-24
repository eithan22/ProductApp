using ProductApp.Aplication.Dtos.ClienteDto;
using ProductApp.Domian.Entitis;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Interface.IMappers.Modulo_Usuarios
{
    public interface IMapperCliente
    {
         ClienteResponseDto MapToClienteResponseDto(Cliente cliente);
          Cliente MapToCreateCliente(CreateClienteDto dto);
          void MapToUpdateCliente(UpdateClienteDto dto, Cliente cliente);
    }
}
