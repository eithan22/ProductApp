using ProductApp.Aplication.Result.OperationResult;

namespace ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Ventas
{
    public interface IValidatorBusinessOrden
    {
        Task<OperationResult> ValidarCrearOrdenAsync(int clienteId);
    }
}
