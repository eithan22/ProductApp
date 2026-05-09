using ProductApp.Aplication.Dtos.ClienteDto;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Entitis;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Interface.RulesBusinnes
{
     public interface IValidatorBusinessClientes
    {
        Task<OperationResult> ValidarCreateClienteAsync(CreateClienteDto dto);
        Task<OperationResult> ValidarUpdateClienteAsync(UpdateClienteDto dto, Cliente cliente);
        Task<OperationResult> ValidarDeleteClienteAsync(Cliente cliente);

        

        



    }
}
