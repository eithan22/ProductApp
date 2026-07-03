using ProductApp.Aplication.Dtos.Modulo_Productos.InventarioDto;
using Web.Models.Modelo_Productos.InventarioModels;

namespace Web.Services.Mappers.Modulo_Productos
{
    public class InventarioMapperM
    {
        public static MovimientoStockDto MapMovimientoStockDto(MovimientoStockModel model)
        {
            return new MovimientoStockDto
            {
                ProductoId = model.ProductoId,
                Cantidad = model.Cantidad
            };
        }

        public static AjustarStockDto MapAjustarStockDto(AjustarStockModel model)
        {
            return new AjustarStockDto
            {
                ProductoId = model.ProductoId,
                NuevoStock = model.NuevoStock
            };
        }
    }
}
