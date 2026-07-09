using ProductApp.Aplication.Common;
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

        Task<OperationResultD<PagedResult<ClienteResponseDto>>> GetAllAsync(bool incluirInactivos, int pageNumber = 1, int pageSize = 10);

        Task<OperationResultD<bool>> EnableCliente(int id);

        Task<OperationResultD<List<ClienteResponseDto>>> BuscarAsync(string? nombre, string? telefono, string? correo, bool incluirInactivos = false);

    }
}
