using FluentValidation;
using Microsoft.Extensions.Logging;
using ProductApp.Aplication.Dtos.Modulo_Ventas.DetalleOrdenDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Interface.IMappers.Modulo_Ventas;
using ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Ventas;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Entitis;
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
        private readonly IGestorTransacciones _gestorTransacciones;
        private readonly ILogger<DetalleOrdenService> _logger;

        public DetalleOrdenService(
            IDetalleOrdenRepository detalleOrdenRepository,
            IOrdenRepository ordenRepository,
            IProductoRepository productoRepository,
            IMapperDetalleOrden mapperDetalleOrden,
            IValidator<CreateDetalleOrdenDto> createDetalleOrdenValidator,
            IValidator<UpdateDetalleOrdenDto> updateDetalleOrdenValidator,
            IValidatorBusinessDetalleOrden validatorBusinessDetalleOrden,
            IGestorTransacciones gestorTransacciones,
            ILogger<DetalleOrdenService> logger)
        {
            _detalleOrdenRepository = detalleOrdenRepository;
            _ordenRepository = ordenRepository;
            _productoRepository = productoRepository;
            _mapperDetalleOrden = mapperDetalleOrden;
            _createDetalleOrdenValidator = createDetalleOrdenValidator;
            _updateDetalleOrdenValidator = updateDetalleOrdenValidator;
            _validatorBusinessDetalleOrden = validatorBusinessDetalleOrden;
            _gestorTransacciones = gestorTransacciones;
            _logger = logger;
        }

        public async Task<OperationResultD<OrdenDetalleResponseDto>> AgregarProductoAsync(CreateDetalleOrdenDto dto, int usuarioSolicitanteId, bool esAdministrador)
        {
            // La validación de forma del DTO no toca la base de datos: se hace antes de abrir la
            // transacción para no mantener bloqueos abiertos de gusto.
            var validatorDto = await _createDetalleOrdenValidator.ValidateAsync(dto);
            if (!validatorDto.IsValid)
                return OperationResultD<OrdenDetalleResponseDto>.Failure(
                    "Error al validar los datos de entrada: " + string.Join(", ", validatorDto.Errors.Select(e => e.ErrorMessage)));

            OrdenDetalle detalleResultante;
            string mensaje;

            try
            {
                // Escribir el detalle y recalcular Orden.Total son una sola unidad: un detalle
                // guardado con el total sin actualizar es una orden que después se cobra y se
                // factura por un monto equivocado. Serializable, además, impide que dos
                // peticiones simultáneas sobre la misma orden (doble clic en "agregar") lean el
                // mismo stock y el mismo conjunto de detalles y las dos se den por válidas.
                await using var transaccion = await _gestorTransacciones.IniciarSerializableAsync();

                // Todas las lecturas van dentro, incluido el guard de propiedad (igual que en
                // PagoService): cualquier return temprano de acá para abajo sale del "await using"
                // sin commit y la transacción se revierte.
                var propiedad = await ValidarPropiedadOrdenAsync(dto.OrdenId, usuarioSolicitanteId, esAdministrador);
                if (!propiedad.IsSuccess)
                    return OperationResultD<OrdenDetalleResponseDto>.Failure(propiedad.Message);

                // El validator de negocio relee estado de la orden, pagos registrados y stock
                // disponible: son justo los datos que otra petición puede estar cambiando, así que
                // va dentro de la transacción y no antes de abrirla.
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

                    detalleResultante = detalleExistente;
                    mensaje = "Cantidad del producto actualizada en el detalle de orden exitosamente";
                }
                else
                {
                    var detalleOrden = _mapperDetalleOrden.MapToCreateDetalleOrden(dto, producto!);
                    await _detalleOrdenRepository.CreateAsync(detalleOrden);

                    var detalleConProducto = await _detalleOrdenRepository.ObtenerConProductoAsync(detalleOrden.Id);
                    if (detalleConProducto == null)
                        return OperationResultD<OrdenDetalleResponseDto>.Failure("Error al obtener el detalle de orden con el producto");

                    var recalculo = await RecalcularTotalOrdenAsync(dto.OrdenId);
                    if (!recalculo.IsSuccess)
                        return OperationResultD<OrdenDetalleResponseDto>.Failure("Error al recalcular el total de la orden: " + recalculo.Message);

                    detalleResultante = detalleConProducto;
                    mensaje = "Producto agregado al detalle de orden exitosamente";
                }

                await transaccion.CommitAsync();
            }
            catch (Exception ex) when (_gestorTransacciones.EsConflictoDeConcurrencia(ex))
            {
                // La transacción perdedora ya fue revertida por el motor: no quedó detalle sin su
                // total recalculado. Se responde con un mensaje de negocio, no un 500.
                _logger.LogWarning(ex,
                    "Conflicto de concurrencia al agregar el producto {ProductId} a la orden {OrdenId}",
                    dto.ProductId, dto.OrdenId);

                return OperationResultD<OrdenDetalleResponseDto>.Failure(
                    "La orden está siendo modificada por otra operación. Vuelva a intentarlo.");
            }

            // El mapeo es en memoria: no necesita la transacción abierta.
            var response = _mapperDetalleOrden.MapToDetalleOrdenResponseDto(detalleResultante);
            return OperationResultD<OrdenDetalleResponseDto>.Success(response, mensaje);
        }

        public async Task<OperationResultD<OrdenDetalleResponseDto>> ActualizarDetalleOrden(int id, UpdateDetalleOrdenDto dto, int usuarioSolicitanteId, bool esAdministrador)
        {
            // Validación de forma: fuera de la transacción, no toca la base.
            var validatorDto = await _updateDetalleOrdenValidator.ValidateAsync(dto);
            if (!validatorDto.IsValid)
                return OperationResultD<OrdenDetalleResponseDto>.Failure(
                    "Error al validar los datos de entrada: " + string.Join(", ", validatorDto.Errors.Select(e => e.ErrorMessage)));

            OrdenDetalle detalleResultante;

            try
            {
                // Cambiar la cantidad del detalle y recalcular Orden.Total van juntos: el total es
                // un dato derivado de los detalles, y dejarlo desfasado se paga al cobrar.
                await using var transaccion = await _gestorTransacciones.IniciarSerializableAsync();

                // El detalle se carga acá y no después del validator porque el dueño de la orden
                // solo se conoce a través de él: sin esto no hay contra quién comparar. Va dentro
                // de la transacción porque es una lectura de la fila que estamos por escribir.
                var detalleOrden = await _detalleOrdenRepository.GetByIdAsync(id);
                if (detalleOrden == null)
                    return OperationResultD<OrdenDetalleResponseDto>.Failure("Detalle de orden no encontrado");

                var propiedad = await ValidarPropiedadOrdenAsync(detalleOrden.OrdenId, usuarioSolicitanteId, esAdministrador);
                if (!propiedad.IsSuccess)
                    return OperationResultD<OrdenDetalleResponseDto>.Failure(propiedad.Message);

                // Estado de la orden, pagos ya registrados y stock disponible: todo relectura de
                // datos que otra petición puede cambiar, así que dentro de la transacción.
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

                await transaccion.CommitAsync();
                detalleResultante = detalleConProducto;
            }
            catch (Exception ex) when (_gestorTransacciones.EsConflictoDeConcurrencia(ex))
            {
                _logger.LogWarning(ex, "Conflicto de concurrencia al actualizar el detalle {DetalleId}", id);

                return OperationResultD<OrdenDetalleResponseDto>.Failure(
                    "La orden está siendo modificada por otra operación. Vuelva a intentarlo.");
            }

            var response = _mapperDetalleOrden.MapToDetalleOrdenResponseDto(detalleResultante);
            return OperationResultD<OrdenDetalleResponseDto>.Success(response, "Detalle de orden actualizado exitosamente");
        }

        public async Task<OperationResultD<bool>> EliminarProductoAsync(int id, int usuarioSolicitanteId, bool esAdministrador)
        {
            // Este método no tiene DTO: no hay nada que validar fuera de la transacción.
            try
            {
                // Borrar el detalle y recalcular el total van juntos: un borrado sin recálculo deja
                // la orden cobrándose productos que ya no tiene.
                await using var transaccion = await _gestorTransacciones.IniciarSerializableAsync();

                var detalleOrden = await _detalleOrdenRepository.GetByIdAsync(id);
                if (detalleOrden == null)
                    return OperationResultD<bool>.Failure("Detalle de orden no encontrado");

                var propiedad = await ValidarPropiedadOrdenAsync(detalleOrden.OrdenId, usuarioSolicitanteId, esAdministrador);
                if (!propiedad.IsSuccess)
                    return OperationResultD<bool>.Failure(propiedad.Message);

                // Estado de la orden y pagos registrados: se releen dentro porque un pago puede
                // entrar justo entre la validación y el borrado.
                var businessResult = await _validatorBusinessDetalleOrden.ValidarEliminarDetalleAsync(id);
                if (!businessResult.IsSuccess)
                    return OperationResultD<bool>.Failure(businessResult.Message);

                await _detalleOrdenRepository.DeleteAsync(detalleOrden.Id);

                var recalculo = await RecalcularTotalOrdenAsync(detalleOrden.OrdenId);
                if (!recalculo.IsSuccess)
                    return OperationResultD<bool>.Failure("Error al recalcular el total de la orden: " + recalculo.Message);

                await transaccion.CommitAsync();
            }
            catch (Exception ex) when (_gestorTransacciones.EsConflictoDeConcurrencia(ex))
            {
                _logger.LogWarning(ex, "Conflicto de concurrencia al eliminar el detalle {DetalleId}", id);

                return OperationResultD<bool>.Failure(
                    "La orden está siendo modificada por otra operación. Vuelva a intentarlo.");
            }

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
