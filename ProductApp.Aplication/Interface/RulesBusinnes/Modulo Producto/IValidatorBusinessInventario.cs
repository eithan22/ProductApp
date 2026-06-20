using ProductApp.Aplication.Dtos.Modulo_Productos.InventarioDto;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Entitis;

namespace ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Producto
{
    public interface IValidatorBusinessInventario
    {
        Task<OperationResult> ValidarEntradaStockAsync(MovimientoStockDto dto, Inventario inventario);
        Task<OperationResult> ValidarSalidaStockAsync(MovimientoStockDto dto, Inventario inventario);
        Task<OperationResult> ValidarAjusteStockAsync(AjustarStockDto dto, Inventario inventario);
    }
}
