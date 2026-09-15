using ProductApp.Aplication.Dtos.Modulo_Proveedores.ProveedorDto;
using ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Proveedores;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Common.Enums.EnumsProveedor;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;

namespace ProductApp.Aplication.BusinessValidator.Modulo_Proveedores
{
    public class ValidatorBusinessProveedor : IValidatorBusinessProveedor
    {
        private readonly IProveedorRepository _proveedorRepository;

        public ValidatorBusinessProveedor(IProveedorRepository proveedorRepository)
        {
            _proveedorRepository = proveedorRepository;
        }

        // Solo nombre y correo son únicos. El teléfono puede repetirse a propósito: es
        // normal que varias sucursales o razones sociales de un mismo grupo compartan
        // la misma central telefónica.
        public async Task<OperationResult> ValidarCreateProveedorAsync(CreateProveedorDto dto)
        {
            if (await _proveedorRepository.ExisteAsync(p => p.Nombre == dto.Nombre))
                return OperationResult.Failure("El nombre ya está registrado por otro proveedor.");

            if (await _proveedorRepository.ExisteAsync(p => p.Correo == dto.Correo))
                return OperationResult.Failure("El correo ya está registrado por otro proveedor.");

            return OperationResult.Success("Validacion Correcta");
        }

        public async Task<OperationResult> ValidarUpdateProveedorAsync(UpdateProveedorDto dto, Proveedor proveedor)
        {
            if (proveedor.Estado == EstadoProveedor.Inactivo)
                return OperationResult.Failure("No se puede actualizar un proveedor inactivo.");

            if (proveedor.Nombre != dto.Nombre && await _proveedorRepository.ExisteAsync(p => p.Nombre == dto.Nombre))
                return OperationResult.Failure("El nombre ya está registrado por otro proveedor.");

            if (proveedor.Correo != dto.Correo && await _proveedorRepository.ExisteAsync(p => p.Correo == dto.Correo))
                return OperationResult.Failure("El correo ya está registrado por otro proveedor.");

            return OperationResult.Success("Validacion Correcta");
        }

        public Task<OperationResult> ValidarDeleteProveedorAsync(Proveedor proveedor)
        {
            if (proveedor.Estado == EstadoProveedor.Inactivo)
                return Task.FromResult(OperationResult.Failure("El proveedor ya está inactivo."));

            return Task.FromResult(OperationResult.Success("Validacion Correcta"));
        }

        // Regla propia del borrado físico: la FK de Producto está en Restrict, así que
        // borrar un proveedor con productos asociados reventaría a nivel de base. Se valida
        // antes para responder con un mensaje entendible.
        public async Task<OperationResult> ValidarBorradoFisicoProveedorAsync(Proveedor proveedor)
        {
            var productosAsociados = await _proveedorRepository.ContarProductosAsociadosAsync(proveedor.Id);

            if (productosAsociados > 0)
                return OperationResult.Failure(
                    $"No se puede eliminar el proveedor porque tiene {productosAsociados} producto(s) asociado(s). Desactívalo en su lugar.");

            return OperationResult.Success("Validacion Correcta");
        }
    }
}
