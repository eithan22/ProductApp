using ProductApp.Aplication.Dtos.ClienteDto;
using ProductApp.Aplication.Interface.BaseServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Interface
{
     public interface IClienteServices : IBaseServices<ClienteResponseDto, CreateClienteDto, UpdateClienteDto>
    {

    }
}
