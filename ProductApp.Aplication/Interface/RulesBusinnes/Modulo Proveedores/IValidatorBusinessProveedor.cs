using ProductApp.Aplication.Dtos.Modulo_Proveedores.ProveedorDto;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Entitis;

namespace ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Proveedores
{
    public interface IValidatorBusinessProveedor
    {
        Task<OperationResult> ValidarCreateProveedorAsync(CreateProveedorDto dto);
        Task<OperationResult> ValidarUpdateProveedorAsync(UpdateProveedorDto dto, Proveedor proveedor);
        Task<OperationResult> ValidarDeleteProveedorAsync(Proveedor proveedor);

        // Regla propia del borrado físico: la FK de Producto está en Restrict, así que
        // borrar un proveedor con productos asociados reventaría a nivel de base. Se valida
        // antes para responder con un mensaje entendible.
        Task<OperationResult> ValidarBorradoFisicoProveedorAsync(Proveedor proveedor);
    }
}
