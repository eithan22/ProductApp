using FluentValidation;
using ProductApp.Aplication.Dtos.Modulo_Ventas.DetalleOrdenDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Interface.IMappers.Modulo_Ventas;
using ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Ventas;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Interfaces;

namespace ProductApp.Aplication.Services
{
    public class DetalleOrdenService : IDetalleOrdenServices
    {
        private readonly IDetalleOrdenRepository _detalleOrdenRepository;
        private readonly IOrdenRepository _ordenRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly IMapperDetalleOrden _mapperDetalleOrden;
        private readonly IValidator<CreateDetalleOrdenDto> _createDetalleOrdenValidator;
        private readonly IValidator<UpdateDetalleOrdenDto> _updateDetalleOrdenValidator;
        private readonly IValidatorBusinessDetalleOrden _validatorBusinessDetalleOrden;

        public DetalleOrdenService(
            IDetalleOrdenRepository detalleOrdenRepository,
            IOrdenRepository ordenRepository,
            IProductoRepository productoRepository,
            IMapperDetalleOrden mapperDetalleOrden,
            IValidator<CreateDetalleOrdenDto> createDetalleOrdenValidator,
            IValidator<UpdateDetalleOrdenDto> updateDetalleOrdenValidator,
            IValidatorBusinessDetalleOrden validatorBusinessDetalleOrden)
        {
            _detalleOrdenRepository = detalleOrdenRepository;
            _ordenRepository = ordenRepository;
            _productoRepository = productoRepository;
            _mapperDetalleOrden = mapperDetalleOrden;
            _createDetalleOrdenValidator = createDetalleOrdenValidator;
            _updateDetalleOrdenValidator = updateDetalleOrdenValidator;
            _validatorBusinessDetalleOrden = validatorBusinessDetalleOrden;
        }

        public async Task<OperationResultD<OrdenDetalleResponseDto>> AgregarProductoAsync(CreateDetalleOrdenDto dto, int usuarioSolicitanteId, bool esAdministrador)
        {
            var validatorDto = await _createDetalleOrdenValidator.ValidateAsync(dto);
            if (!validatorDto.IsValid)
                return OperationResultD<OrdenDetalleResponseDto>.Failure(
                    "Error al validar los datos de entrada: " + string.Join(", ", validatorDto.Errors.Select(e => e.ErrorMessage)));

            var propiedad = await ValidarPropiedadOrdenAsync(dto.OrdenId, usuarioSolicitanteId, esAdministrador);
            if (!propiedad.IsSuccess)
                return OperationResultD<OrdenDetalleResponseDto>.Failure(propiedad.Message);

            var businessResult = await _validatorBusinessDetalleOrden.ValidarAgregarProductoAsync(dto);
            if (!businessResult.IsSuccess)
                return OperationResultD<OrdenDetalleResponseDto>.Failure(businessResult.Message);

            var producto = await _productoRepository.ObtenerConInventarioAsync(dto.ProductId);
            var detalleExistente = await _detalleOrdenRepository.ObtenerProductoEnOrdenAsync(dto.OrdenId, dto.ProductId);

            if (detalleExistente != null)
            {
                detalleExistente.ActualizarCantidad(detalleExistente.Cantidad + dto.Cantidad);
                await _detalleOrdenRepository.UpdateAsync(detalleExistente);

                var recalculoActualizado = await RecalcularTotalOrdenAsync(dto.OrdenId);
                if (!recalculoActualizado.IsSuccess)
                    return OperationResultD<OrdenDetalleResponseDto>.Failure("Error al recalcular el total de la orden: " + recalculoActualizado.Message);

                var responseActualizado = _mapperDetalleOrden.MapToDetalleOrdenResponseDto(detalleExistente);
                return OperationResultD<OrdenDetalleResponseDto>.Success(responseActualizado, "Cantidad del producto actualizada en el detalle de orden exitosamente");
            }

            var detalleOrden = _mapperDetalleOrden.MapToCreateDetalleOrden(dto, producto!);
            await _detalleOrdenRepository.CreateAsync(detalleOrden);

            var detalleConProducto = await _detalleOrdenRepository.ObtenerConProductoAsync(detalleOrden.Id);
            if (detalleConProducto == null)
                return OperationResultD<OrdenDetalleResponseDto>.Failure("Error al obtener el detalle de orden con el producto");

            var recalculo = await RecalcularTotalOrdenAsync(dto.OrdenId);
            if (!recalculo.IsSuccess)
                return OperationResultD<OrdenDetalleResponseDto>.Failure("Error al recalcular el total de la orden: " + recalculo.Message);

            var response = _mapperDetalleOrden.MapToDetalleOrdenResponseDto(detalleConProducto);
            return OperationResultD<OrdenDetalleResponseDto>.Success(response, "Producto agregado al detalle de orden exitosamente");
        }

        public async Task<OperationResultD<OrdenDetalleResponseDto>> ActualizarDetalleOrden(int id, UpdateDetalleOrdenDto dto, int usuarioSolicitanteId, bool esAdministrador)
        {
            var validatorDto = await _updateDetalleOrdenValidator.ValidateAsync(dto);
            if (!validatorDto.IsValid)
                return OperationResultD<OrdenDetalleResponseDto>.Failure(
                    "Error al validar los datos de entrada: " + string.Join(", ", validatorDto.Errors.Select(e => e.ErrorMessage)));

            // El detalle se carga acá y no después del validator porque el dueño de la orden
            // solo se conoce a través de él: sin esto no hay contra quién comparar.
            var detalleOrden = await _detalleOrdenRepository.GetByIdAsync(id);
            if (detalleOrden == null)
                return OperationResultD<OrdenDetalleResponseDto>.Failure("Detalle de orden no encontrado");

            var propiedad = await ValidarPropiedadOrdenAsync(detalleOrden.OrdenId, usuarioSolicitanteId, esAdministrador);
            if (!propiedad.IsSuccess)
                return OperationResultD<OrdenDetalleResponseDto>.Failure(propiedad.Message);

            var businessResult = await _validatorBusinessDetalleOrden.ValidarActualizarDetalleAsync(id, dto);
            if (!businessResult.IsSuccess)
                return OperationResultD<OrdenDetalleResponseDto>.Failure(businessResult.Message);

            _mapperDetalleOrden.MapToUpdateDetalleOrden(dto, detalleOrden);
            await _detalleOrdenRepository.UpdateAsync(detalleOrden);

            var recalculo = await RecalcularTotalOrdenAsync(detalleOrden.OrdenId);
            if (!recalculo.IsSuccess)
                return OperationResultD<OrdenDetalleResponseDto>.Failure("Error al recalcular el total de la orden: " + recalculo.Message);

            var detalleConProducto = await _detalleOrdenRepository.ObtenerConProductoAsync(detalleOrden.Id);
            if (detalleConProducto == null)
                return OperationResultD<OrdenDetalleResponseDto>.Failure("Error al obtener el detalle de orden con el producto");

            var response = _mapperDetalleOrden.MapToDetalleOrdenResponseDto(detalleConProducto);
            return OperationResultD<OrdenDetalleResponseDto>.Success(response, "Detalle de orden actualizado exitosamente");
        }

        public async Task<OperationResultD<bool>> EliminarProductoAsync(int id, int usuarioSolicitanteId, bool esAdministrador)
        {
            var detalleOrden = await _detalleOrdenRepository.GetByIdAsync(id);
            if (detalleOrden == null)
                return OperationResultD<bool>.Failure("Detalle de orden no encontrado");

            var propiedad = await ValidarPropiedadOrdenAsync(detalleOrden.OrdenId, usuarioSolicitanteId, esAdministrador);
            if (!propiedad.IsSuccess)
                return OperationResultD<bool>.Failure(propiedad.Message);

            var businessResult = await _validatorBusinessDetalleOrden.ValidarEliminarDetalleAsync(id);
            if (!businessResult.IsSuccess)
                return OperationResultD<bool>.Failure(businessResult.Message);

            await _detalleOrdenRepository.DeleteAsync(detalleOrden.Id);

            var recalculo = await RecalcularTotalOrdenAsync(detalleOrden.OrdenId);
            if (!recalculo.IsSuccess)
                return OperationResultD<bool>.Failure("Error al recalcular el total de la orden: " + recalculo.Message);

            return OperationResultD<bool>.Success(true, "Producto eliminado del detalle de orden exitosamente");
        }

        public async Task<OperationResultD<List<OrdenDetalleResponseDto>>> GetOrdenDetalle(int id)
        {
            var detalles = await _detalleOrdenRepository.ObtenerPorOrdenIdAsync(id);
            if (!detalles.Any())
                return OperationResultD<List<OrdenDetalleResponseDto>>.Success(new List<OrdenDetalleResponseDto>(), "Esta orden no tiene productos todavía");

            var detallesResponse = detalles.Select(d => _mapperDetalleOrden.MapToDetalleOrdenResponseDto(d)).ToList();
            return OperationResultD<List<OrdenDetalleResponseDto>>.Success(detallesResponse, "Detalles de la orden obtenidos exitosamente");
        }

        private async Task<OperationResultD<bool>> RecalcularTotalOrdenAsync(int ordenId)
        {
            var orden = await _ordenRepository.GetByIdAsync(ordenId);
            if (orden == null)
                return OperationResultD<bool>.Failure("Orden no encontrada");

            var detalles = await _detalleOrdenRepository.ObtenerPorOrdenIdAsync(ordenId);
            orden.ActualizarTotal(detalles.Sum(d => d.Subtotal));
            await _ordenRepository.UpdateAsync(orden);

            return OperationResultD<bool>.Success(true, "Total recalculado");
        }

        // Mismo guard que OrdenServices.CancelarOrden y PagoService.RegistrarPagoAsync: el
        // carrito pendiente de otro vendedor no se toca. Va en el servicio y no en el
        // validator de negocio porque es una pregunta de permisos, no de reglas de venta.
        private async Task<OperationResult> ValidarPropiedadOrdenAsync(int ordenId, int usuarioSolicitanteId, bool esAdministrador)
        {
            var orden = await _ordenRepository.GetByIdAsync(ordenId);
            if (orden == null)
                return OperationResult.Failure("Orden no encontrada");

            if (!esAdministrador && orden.UsuarioId != usuarioSolicitanteId)
                return OperationResult.Failure("No tiene permiso sobre esta orden");

            return OperationResult.Success();
        }
    }
}
