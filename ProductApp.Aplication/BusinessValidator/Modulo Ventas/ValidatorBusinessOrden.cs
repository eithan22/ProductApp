using ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Ventas;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Common.Enums.EnumsCliente;
using ProductApp.Domian.Common.Enums.EnumsOrden;
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

        public Task<OperationResult> ValidarCambiarEstadoAsync(EstadoOrden nuevoEstado)
        {
            if (nuevoEstado == EstadoOrden.Pagada)
                return Task.FromResult(OperationResult.Failure(
                    "El estado 'Pagada' no se puede asignar manualmente; se establece automáticamente al registrar un pago completo."));

            if (nuevoEstado == EstadoOrden.Procesada)
                return Task.FromResult(OperationResult.Failure(
                    "El estado 'Procesada' no se puede asignar manualmente; use el endpoint ConfirmarOrden."));

            return Task.FromResult(OperationResult.Success());
        }

        // Un pago parcial deja la orden en Pendiente, así que la máquina de estados no impide
        // cancelarla y el dinero ya cobrado se queda sin contraparte. Esa decisión solo la puede
        // tomar un administrador; el servicio la deja auditada.
        public Task<OperationResult> ValidarCancelarOrdenAsync(decimal totalPagado, bool esAdministrador)
        {
            if (totalPagado > 0 && !esAdministrador)
                return Task.FromResult(OperationResult.Failure(
                    "Esta orden tiene pagos registrados; solicite a un administrador la cancelación."));

            return Task.FromResult(OperationResult.Success());
        }
    }
}
