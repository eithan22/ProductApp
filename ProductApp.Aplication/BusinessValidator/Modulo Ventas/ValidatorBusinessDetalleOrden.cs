using ProductApp.Aplication.Dtos.Modulo_Ventas.DetalleOrdenDto;
using ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Ventas;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Common.Enums.EnumsOrden;
using ProductApp.Domian.Common.Enums.EnumsProducto;
using ProductApp.Domian.Interfaces;

namespace ProductApp.Aplication.BusinessValidator.Modulo_Ventas
{
    public class ValidatorBusinessDetalleOrden : IValidatorBusinessDetalleOrden
    {
        private readonly IOrdenRepository _ordenRepository;
        private readonly IDetalleOrdenRepository _detalleOrdenRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly IPagoRepository _pagoRepository;

        public ValidatorBusinessDetalleOrden(
            IOrdenRepository ordenRepository,
            IDetalleOrdenRepository detalleOrdenRepository,
            IProductoRepository productoRepository,
            IPagoRepository pagoRepository)
        {
            _ordenRepository = ordenRepository;
            _detalleOrdenRepository = detalleOrdenRepository;
            _productoRepository = productoRepository;
            _pagoRepository = pagoRepository;
        }

        public async Task<OperationResult> ValidarAgregarProductoAsync(CreateDetalleOrdenDto dto)
        {
            var orden = await _ordenRepository.GetByIdAsync(dto.OrdenId);
            if (orden == null)
                return OperationResult.Failure("Orden no encontrada");

            if (orden.Estado != EstadoOrden.Pendiente)
                return OperationResult.Failure("No se pueden agregar productos a una orden que no está pendiente");

            if (await _pagoRepository.ExisteAsync(p => p.OrdenId == orden.Id))
                return OperationResult.Failure("No se pueden agregar productos a una orden que ya tiene pagos registrados");

            var producto = await _productoRepository.ObtenerConInventarioAsync(dto.ProductId);
            if (producto == null)
                return OperationResult.Failure("Producto no encontrado");

            if (producto.Inventario == null)
                return OperationResult.Failure("Inventario no encontrado para este producto");

            if (producto.Estado == EstadoProducto.Inactivo)
                return OperationResult.Failure("No se puede agregar un producto desactivado a una orden");

            var detalleExistente = await _detalleOrdenRepository.ObtenerProductoEnOrdenAsync(dto.OrdenId, dto.ProductId);
            var cantidadTotal = detalleExistente != null ? detalleExistente.Cantidad + dto.Cantidad : dto.Cantidad;

            if (cantidadTotal > producto.Inventario.CantidadActual)
                return OperationResult.Failure("La cantidad solicitada excede el stock disponible");

            return OperationResult.Success();
        }

        public async Task<OperationResult> ValidarActualizarDetalleAsync(int detalleId, UpdateDetalleOrdenDto dto)
        {
            var detalle = await _detalleOrdenRepository.GetByIdAsync(detalleId);
            if (detalle == null)
                return OperationResult.Failure("Detalle de orden no encontrado");

            var orden = await _ordenRepository.GetByIdAsync(detalle.OrdenId);
            if (orden == null)
                return OperationResult.Failure("Orden no encontrada");

            if (orden.Estado != EstadoOrden.Pendiente)
                return OperationResult.Failure("No se pueden modificar detalles de una orden que no está pendiente");

            if (await _pagoRepository.ExisteAsync(p => p.OrdenId == orden.Id))
                return OperationResult.Failure("No se pueden modificar detalles de una orden que ya tiene pagos registrados");

            var producto = await _productoRepository.ObtenerConInventarioAsync(detalle.ProductId);
            if (producto == null)
                return OperationResult.Failure("Producto no encontrado");

            if (producto.Inventario == null)
                return OperationResult.Failure("Inventario no encontrado para este producto");

            if (dto.Cantidad > producto.Inventario.CantidadActual)
                return OperationResult.Failure("Cantidad solicitada excede el stock disponible");

            return OperationResult.Success();
        }

        public async Task<OperationResult> ValidarEliminarDetalleAsync(int detalleId)
        {
            var detalle = await _detalleOrdenRepository.GetByIdAsync(detalleId);
            if (detalle == null)
                return OperationResult.Failure("Detalle de orden no encontrado");

            var orden = await _ordenRepository.GetByIdAsync(detalle.OrdenId);
            if (orden == null)
                return OperationResult.Failure("Orden no encontrada");

            if (orden.Estado != EstadoOrden.Pendiente)
                return OperationResult.Failure("No se pueden eliminar productos de una orden que no está pendiente");

            if (await _pagoRepository.ExisteAsync(p => p.OrdenId == orden.Id))
                return OperationResult.Failure("No se pueden eliminar productos de una orden que ya tiene pagos registrados");

            return OperationResult.Success();
        }
    }
}
