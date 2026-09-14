using FluentValidation;
using Microsoft.Extensions.Logging;
using ProductApp.Aplication.Common;
using ProductApp.Aplication.Dtos.PagoDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Interface.IMappers.Modulo_Ventas;
using ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Ventas;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Common.Enums.EnumsNotificacion;
using ProductApp.Domian.Common.Enums.EnumsOrden;
using ProductApp.Domian.Common.Enums.EnumsPago;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;

namespace ProductApp.Aplication.Services
{
    public class PagoService : IPagoServices
    {
        private readonly IPagoRepository _pagoRepository;
        private readonly IOrdenRepository _ordenRepository;
        private readonly IDetalleOrdenRepository _detalleOrdenRepository;
        private readonly IInventarioRepository _inventarioRepository;
        private readonly IMapperPago _mapperPago;
        private readonly IValidator<CreatePagoDto> _createPagoValidator;
        private readonly IValidatorBusinessPago _validatorBusinessPago;
        private readonly INotificacionServices _notificacionServices;
        private readonly IFacturaPdfService _facturaPdfService;
        private readonly ILogger<PagoService> _logger;

        public PagoService(
            IPagoRepository pagoRepository,
            IOrdenRepository ordenRepository,
            IDetalleOrdenRepository detalleOrdenRepository,
            IInventarioRepository inventarioRepository,
            IMapperPago mapperPago,
            IValidator<CreatePagoDto> createPagoValidator,
            IValidatorBusinessPago validatorBusinessPago,
            INotificacionServices notificacionServices,
            IFacturaPdfService facturaPdfService,
            ILogger<PagoService> logger)
        {
            _pagoRepository = pagoRepository;
            _ordenRepository = ordenRepository;
            _detalleOrdenRepository = detalleOrdenRepository;
            _inventarioRepository = inventarioRepository;
            _mapperPago = mapperPago;
            _createPagoValidator = createPagoValidator;
            _validatorBusinessPago = validatorBusinessPago;
            _notificacionServices = notificacionServices;
            _facturaPdfService = facturaPdfService;
            _logger = logger;
        }

        public async Task<OperationResultD<PagoResponseDto>> RegistrarPagoAsync(CreatePagoDto dto, int usuarioSolicitanteId)
        {
            var validationResult = await _createPagoValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return OperationResultD<PagoResponseDto>.Failure(
                    string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

            var orden = await _ordenRepository.GetByIdAsync(dto.OrdenId);
            if (orden == null)
                return OperationResultD<PagoResponseDto>.Failure("Orden no encontrada");

            var totalPagado = await _pagoRepository.ObtenerTotalPagadoPorOrdenAsync(dto.OrdenId);
            var saldoActual = orden.Total - totalPagado;

            var businessResult = await _validatorBusinessPago.ValidarRegistrarPagoAsync(dto, orden, saldoActual);
            if (!businessResult.IsSuccess)
                return OperationResultD<PagoResponseDto>.Failure(businessResult.Message);

            var nuevoSaldo = saldoActual - dto.Monto;
            var pagoCompleto = nuevoSaldo <= 0;

            var inventariosADescontar = new List<(Inventario inventario, int cantidad)>();
            if (pagoCompleto)
            {
                var detalles = await _detalleOrdenRepository.ObtenerPorOrdenIdAsync(dto.OrdenId);
                foreach (var detalle in detalles)
                {
                    var inventario = await _inventarioRepository.GetByProductoIdAsync(detalle.ProductId);
                    if (inventario == null)
                        return OperationResultD<PagoResponseDto>.Failure(
                            $"Inventario no encontrado para el producto con Id {detalle.ProductId}");

                    if (detalle.Cantidad > inventario.CantidadActual)
                        return OperationResultD<PagoResponseDto>.Failure(
                            $"Stock insuficiente para el producto con Id {detalle.ProductId}. " +
                            $"Disponible: {inventario.CantidadActual}, requerido: {detalle.Cantidad}");

                    inventariosADescontar.Add((inventario, detalle.Cantidad));
                }
            }

            var pago = _mapperPago.MapToCreatePago(dto);
            await _pagoRepository.CreateAsync(pago);

            if (pagoCompleto)
            {
                pago.MarcarComoCompletado();
                await _pagoRepository.UpdateAsync(pago);

                orden.CambiarEstado(EstadoOrden.Pagada);
                await _ordenRepository.UpdateAsync(orden);

                foreach (var (inventario, cantidad) in inventariosADescontar)
                {
                    var estabaBajo = inventario.EsStockBajo();

                    inventario.RegistrarSalidaStock(cantidad);
                    await _inventarioRepository.UpdateAsync(inventario);

                    if (!estabaBajo && inventario.EsStockBajo())
                    {
                        await _notificacionServices.NotificarAdministradoresAsync(
                            TipoNotificacion.StockBajo,
                            $"Stock bajo: \"{inventario.Producto.Nombre}\" quedó en {inventario.CantidadActual} unidades (mínimo {inventario.CantidadMinima}).");
                    }
                }

                // La factura se emite en el mismo flujo en que la orden queda Pagada (RF-3.2.2):
                // una sola vez y archivada en Blob Storage, no regenerada en cada descarga.
                var factura = await _facturaPdfService.GenerarYAlmacenarAsync(orden.Id);
                if (!factura.IsSuccess)
                {
                    // El cobro ya se registró: un fallo al emitir el PDF no puede revertirlo ni
                    // ocultarlo. Queda el log para regenerar la factura manualmente.
                    _logger.LogError(
                        "El pago de la orden {OrdenId} se registró correctamente, pero la factura PDF no pudo emitirse: {Motivo}",
                        orden.Id, factura.Message);
                }
            }

            var mensaje = pagoCompleto
                ? "Pago registrado. La orden ha sido marcada como Pagada e inventario descontado"
                : $"Pago parcial registrado exitosamente. Saldo pendiente: {nuevoSaldo}";

            _logger.LogInformation("Pago registrado para la orden {OrdenId}: monto {Monto}, pago completo: {PagoCompleto}, por el usuario {UsuarioSolicitanteId}", dto.OrdenId, dto.Monto, pagoCompleto, usuarioSolicitanteId);

            await _notificacionServices.NotificarUsuarioAsync(
                orden.UsuarioId,
                TipoNotificacion.PagoRegistrado,
                pagoCompleto
                    ? $"La orden #{orden.Id} fue pagada completamente."
                    : $"Se registró un pago de {dto.Monto} para la orden #{orden.Id}. Saldo pendiente: {nuevoSaldo}.");

            // Los administradores se enteran de todo dinero que entra, no solo el vendedor dueño
            // de la orden (RF-3.6.1). Se excluye a orden.UsuarioId para no duplicarle la
            // notificación si ese vendedor además tiene rol Administrador.
            await _notificacionServices.NotificarAdministradoresAsync(
                TipoNotificacion.PagoRegistrado,
                pagoCompleto
                    ? $"Pago completo de {dto.Monto} registrado en la orden #{orden.Id}. La orden quedó Pagada."
                    : $"Pago de {dto.Monto} registrado en la orden #{orden.Id}. Saldo pendiente: {nuevoSaldo}.",
                orden.UsuarioId);

            var response = _mapperPago.MapToPagoResponseDto(pago, nuevoSaldo);
            return OperationResultD<PagoResponseDto>.Success(response, mensaje);
        }

        public async Task<OperationResultD<List<PagoResponseDto>>> ObtenerPagosPorOrdenAsync(int ordenId)
        {
            var orden = await _ordenRepository.GetByIdAsync(ordenId);
            if (orden == null)
                return OperationResultD<List<PagoResponseDto>>.Failure("Orden no encontrada");

            var pagos = await _pagoRepository.ObtenerPagosPorOrdenAsync(ordenId);
            if (!pagos.Any())
                return OperationResultD<List<PagoResponseDto>>.Success(new List<PagoResponseDto>(), "Esta orden no tiene pagos todavía");

            var totalPagado = pagos.Sum(p => p.Monto);
            var saldoPendiente = orden.Total - totalPagado;

            var response = pagos.Select(p => _mapperPago.MapToPagoResponseDto(p, saldoPendiente)).ToList();
            return OperationResultD<List<PagoResponseDto>>.Success(response, "Pagos obtenidos exitosamente");
        }

        public async Task<OperationResultD<decimal>> ObtenerSaldoPendienteAsync(int ordenId)
        {
            var orden = await _ordenRepository.GetByIdAsync(ordenId);
            if (orden == null)
                return OperationResultD<decimal>.Failure("Orden no encontrada");

            var totalPagado = await _pagoRepository.ObtenerTotalPagadoPorOrdenAsync(ordenId);
            var saldoPendiente = orden.Total - totalPagado;

            return OperationResultD<decimal>.Success(saldoPendiente, "Saldo pendiente obtenido exitosamente");
        }

        public async Task<OperationResultD<PagoListadoResponseDto>> ObtenerPagosAsync(PagoFiltroDto filtro)
        {
            if (filtro.PageNumber < 1)
                return OperationResultD<PagoListadoResponseDto>.Failure("pageNumber debe ser mayor o igual a 1");

            if (filtro.PageSize < 1 || filtro.PageSize > 100)
                return OperationResultD<PagoListadoResponseDto>.Failure("pageSize debe estar entre 1 y 100");

            if (filtro.Desde.HasValue && filtro.Hasta.HasValue && filtro.Desde.Value.Date > filtro.Hasta.Value.Date)
                return OperationResultD<PagoListadoResponseDto>.Failure("La fecha 'Desde' no puede ser mayor que la fecha 'Hasta'");

            MetodoPago? metodoPago = null;
            if (!string.IsNullOrWhiteSpace(filtro.MetodoPago))
            {
                if (!Enum.TryParse<MetodoPago>(filtro.MetodoPago, true, out var metodoParseado))
                    return OperationResultD<PagoListadoResponseDto>.Failure(
                        $"El método de pago '{filtro.MetodoPago}' no es válido. Valores permitidos: {string.Join(", ", Enum.GetNames<MetodoPago>())}");

                metodoPago = metodoParseado;
            }

            var (pagos, totalCount, totalMonto, ordenesSaldadas, montoPorMetodo, idsPrimerPago) =
                await _pagoRepository.ObtenerPagosPaginadosAsync(
                    filtro.OrdenId, filtro.Desde, filtro.Hasta, metodoPago, filtro.PageNumber, filtro.PageSize);

            var items = pagos.Select(p => _mapperPago.MapToPagoListaResponseDto(p)).ToList();
            foreach (var item in items)
            {
                item.EsPrimerPago = idsPrimerPago.Contains(item.Id);
            }

            var pagedResult = new PagedResult<PagoListaResponseDto>
            {
                Items = items,
                PageNumber = filtro.PageNumber,
                PageSize = filtro.PageSize,
                TotalCount = totalCount
            };

            // El desglose por método se calcula sobre el monto total filtrado, no
            // sobre la página actual, para que el porcentaje sea representativo.
            var porMetodo = montoPorMetodo
                .Select(kv => new PagoMetodoResumenDto
                {
                    MetodoPago = kv.Key.ToString(),
                    Porcentaje = totalMonto > 0 ? Math.Round(kv.Value / totalMonto * 100, 0) : 0
                })
                .OrderByDescending(m => m.Porcentaje)
                .ToList();

            var response = new PagoListadoResponseDto
            {
                Pagos = pagedResult,
                TotalRecibido = totalMonto,
                CantidadPagos = totalCount,
                OrdenesSaldadas = ordenesSaldadas,
                PorMetodo = porMetodo
            };

            return OperationResultD<PagoListadoResponseDto>.Success(response, "Pagos obtenidos exitosamente");
        }
    }
}
