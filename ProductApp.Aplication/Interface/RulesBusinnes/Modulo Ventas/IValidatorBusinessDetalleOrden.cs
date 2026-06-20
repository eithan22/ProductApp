using ProductApp.Aplication.Dtos.Modulo_Ventas.DetalleOrdenDto;
using ProductApp.Aplication.Result.OperationResult;

namespace ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Ventas
{
    public interface IValidatorBusinessDetalleOrden
    {
        Task<OperationResult> ValidarAgregarProductoAsync(CreateDetalleOrdenDto dto);
        Task<OperationResult> ValidarActualizarDetalleAsync(int detalleId, UpdateDetalleOrdenDto dto);
        Task<OperationResult> ValidarEliminarDetalleAsync(int detalleId);
    }
}
