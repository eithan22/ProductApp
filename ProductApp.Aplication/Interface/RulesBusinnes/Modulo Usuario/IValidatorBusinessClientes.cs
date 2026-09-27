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

        // Sin sufijo Async y sin Task: no consulta la base, solo inspecciona la entidad
        // que el servicio ya trajo. Se expone en la interfaz para que el borrado físico
        // pueda reusarla sin arrastrar las otras reglas de la desactivación.
        OperationResult ValidarClienteNoReservado(Cliente cliente);

        OperationResult ValidarAnonimizarClienteAsync(Cliente cliente);

        

        



    }
}
