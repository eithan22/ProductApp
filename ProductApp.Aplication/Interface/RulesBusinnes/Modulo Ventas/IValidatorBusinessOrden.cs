using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Common.Enums.EnumsOrden;

namespace ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Ventas
{
    public interface IValidatorBusinessOrden
    {
        Task<OperationResult> ValidarCrearOrdenAsync(int clienteId);
        Task<OperationResult> ValidarCambiarEstadoAsync(EstadoOrden nuevoEstado);
    }
}
