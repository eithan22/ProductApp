using ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Ventas;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Common.Enums.EnumsCliente;
using ProductApp.Domian.Interfaces;

namespace ProductApp.Aplication.BusinessValidator.Modulo_Ventas
{
    public class ValidatorBusinessOrden : IValidatorBusinessOrden
    {
        private readonly IClienteRepository _clienteRepository;

        public ValidatorBusinessOrden(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
        }

        public async Task<OperationResult> ValidarCrearOrdenAsync(int clienteId)
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);
            if (cliente == null)
                return OperationResult.Failure("El cliente no existe.");

            if (cliente.Estado == EstadoCliente.Inactivo)
                return OperationResult.Failure("No se puede crear una orden para un cliente inactivo.");

            return OperationResult.Success();
        }
    }
}
