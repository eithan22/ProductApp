using ProductApp.Aplication.Dtos.ClienteDto;
using ProductApp.Aplication.Interface.Servicios.BaseServices;
using ProductApp.Aplication.Result.OperationResult;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Interface
{
     public interface IClienteServices : IBaseServices<ClienteResponseDto, CreateClienteDto, UpdateClienteDto>
    {

        Task<OperationResultD<List<ClienteResponseDto>>> BuscarAsync(string? nombre, string? telefono, string? correo);

    }
}
