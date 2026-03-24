using ProductApp.Aplication.Dtos.ClienteDto;
using ProductApp.Domian.Entitis;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Interface.RulesBusinnes
{
     public interface IValidatorBusinessClientes
    {
        Task ValidarCreateClienteAsync(CreateClienteDto dto);
        Task ValidarUpdateClienteAsync(UpdateClienteDto dto, Cliente cliente);
        Task ValidarDeleteClienteAsync(Cliente cliente);

        



    }
}
