using ProductApp.Aplication.Dtos.Modulo_Productos.InventarioDto;
using ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Producto;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Common.Enums.EnumsProducto;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;

namespace ProductApp.Aplication.BusinessValidator.Modulo_Productos
{
    public class ValidatorBusinessInventario : IValidatorBusinessInventario
    {
        private readonly IProductoRepository _productoRepository;

        public ValidatorBusinessInventario(IProductoRepository productoRepository)
        {
            _productoRepository = productoRepository;
        }

        public async Task<OperationResult> ValidarEntradaStockAsync(MovimientoStockDto dto, Inventario inventario)
        {
            var producto = await _productoRepository.GetByIdAsync(inventario.ProductoId);
            if (producto == null)
                return OperationResult.Failure("El producto asociado al inventario no existe.");

            if (producto.Estado == EstadoProducto.Inactivo)
                return OperationResult.Failure("No se puede agregar stock a un producto inactivo.");

            return OperationResult.Success();
        }

        public async Task<OperationResult> ValidarSalidaStockAsync(MovimientoStockDto dto, Inventario inventario)
        {
            var producto = await _productoRepository.GetByIdAsync(inventario.ProductoId);
            if (producto == null)
                return OperationResult.Failure("El producto asociado al inventario no existe.");

            if (producto.Estado == EstadoProducto.Inactivo)
                return OperationResult.Failure("No se puede descontar stock de un producto inactivo.");

            if (dto.Cantidad > inventario.CantidadActual)
                return OperationResult.Failure(
                    $"Stock insuficiente. Disponible: {inventario.CantidadActual}, solicitado: {dto.Cantidad}.");

            return OperationResult.Success();
        }

        public async Task<OperationResult> ValidarAjusteStockAsync(AjustarStockDto dto, Inventario inventario)
        {
            var producto = await _productoRepository.GetByIdAsync(inventario.ProductoId);
            if (producto == null)
                return OperationResult.Failure("El producto asociado al inventario no existe.");

            if (producto.Estado == EstadoProducto.Inactivo)
                return OperationResult.Failure("No se puede ajustar el stock de un producto inactivo.");

            // El mínimo se comprueba acá, antes de que el servicio toque la entidad: el ajuste
            // son dos mutaciones seguidas (cantidad y mínimo) y si la segunda reventara dentro
            // del dominio, el inventario ya habría quedado a medio ajustar en memoria. Mismo
            // criterio que ValidarSalidaStockAsync, que también repite una regla del dominio
            // para poder cortar con un mensaje de negocio en vez de una excepción.
            if (dto.NuevoStockMinimo.HasValue && dto.NuevoStockMinimo.Value < 0)
                return OperationResult.Failure("El nuevo stock mínimo no puede ser negativo.");

            return OperationResult.Success();
        }
    }
}
