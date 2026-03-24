using ProductApp.Aplication.Dtos.ClienteDto;
using ProductApp.Aplication.Interface.IMappers.Modulo_Usuarios;
using ProductApp.Domian.Common.Enums.EnumsCliente;
using ProductApp.Domian.Entitis;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Mappers
{
public class ClienteMappers : IMapperCliente
{
    public ClienteResponseDto MapToClienteResponseDto(Cliente cliente)
    {
        var clienteResponse = new ClienteResponseDto
        {
            Id = cliente.Id,
            Nombre = cliente.Nombre,
            Email = cliente.Correo,
            Telefono = cliente.Telefono,
            Cedula = cliente.Cedula,
            Direccion = cliente.Direccion,
            Estado = cliente.Estado

        };

        return clienteResponse;
    }

    public Cliente MapToCreateCliente(CreateClienteDto dto)
    {
        return new Cliente(

            dto.Nombre,
            dto.Cedula,
            dto.Direccion,
            dto.Correo,
            dto.Telefono
            );

  

    }

    public void MapToUpdateCliente(UpdateClienteDto dto, Cliente cliente)
    {
        cliente.CambiarYvalidarNombre(dto.Nombre);
        cliente.CambiarYvalidarCorreo(dto.Correo);
        cliente.CambiarYvalidarTelefono(dto.Telefono);
        cliente.CambiarYvalidarCedula(dto.Cedula);
        cliente.CambiarYvalidarDireccion(dto.Direccion);
          
          
    }
}
}
