using ProductApp.Aplication.Dtos.ClienteDto;
using ProductApp.Aplication.Interface.RulesBusinnes;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Common.Enums.EnumsCliente;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;

namespace ProductApp.Aplication.BusinessValidator.Modulo_Usuarios
{
    public class ValidatorBusinessClientes : IValidatorBusinessClientes
    {
        private readonly IClienteRepository _clienteRepository;

        public ValidatorBusinessClientes(IClienteRepository clienteRepository)
        {
            _clienteRepository = clienteRepository;
        }

        public async Task<OperationResult> ValidarCreateClienteAsync(CreateClienteDto dto)
        {
            if (await _clienteRepository.ExisteAsync(c => c.Correo == dto.Correo))
                return OperationResult.Failure("El correo ya está registrado.");

            if (await _clienteRepository.ExisteAsync(c => c.Cedula == dto.Cedula))
                return OperationResult.Failure("La cédula ya está registrada.");

            if (await _clienteRepository.ExisteAsync(c => c.Telefono == dto.Telefono))
                return OperationResult.Failure("El teléfono ya está registrado.");

            return OperationResult.Success("Validacion Correcta");
        }

        public async Task<OperationResult> ValidarDeleteClienteAsync(Cliente cliente)
        {
            if (cliente.Estado == EstadoCliente.Inactivo)
                return OperationResult.Failure("El cliente ya está inactivo.");

            return OperationResult.Success("Validacion Correcta");
        }

        public async Task<OperationResult> ValidarUpdateClienteAsync(UpdateClienteDto dto, Cliente cliente)
        {
            if (cliente.Estado == EstadoCliente.Inactivo)
                return OperationResult.Failure("No se puede actualizar un cliente inactivo.");

            if (cliente.Correo != dto.Correo && await _clienteRepository.ExisteAsync(c => c.Correo == dto.Correo))
                return OperationResult.Failure("El correo ya está registrado por otro cliente.");

            if (cliente.Cedula != dto.Cedula && await _clienteRepository.ExisteAsync(c => c.Cedula == dto.Cedula))
                return OperationResult.Failure("La cédula ya está registrada por otro cliente.");

            if (cliente.Telefono != dto.Telefono && await _clienteRepository.ExisteAsync(c => c.Telefono == dto.Telefono))
                return OperationResult.Failure("El teléfono ya está registrado por otro cliente.");

            if (cliente.Nombre != dto.Nombre && await _clienteRepository.ExisteAsync(c => c.Nombre == dto.Nombre))
                return OperationResult.Failure("El nombre ya está registrado por otro cliente.");

            return OperationResult.Success("Validacion Correcta");
        }
    }
}
