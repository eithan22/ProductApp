using FluentValidation;
using Microsoft.Extensions.Logging;
using ProductApp.Aplication.Dtos.Modulo_Ventas.OrdenDto;
using ProductApp.Aplication.Dtos.OrdenDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Interface.IMappers.Modulo_Ventas;
using ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Ventas;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Common.Enums.EnumsNotificacion;
using ProductApp.Domian.Common.Enums.EnumsOrden;
using ProductApp.Domian.Interfaces;

namespace ProductApp.Aplication.Services
{
    public class OrdenServices : IOrdenServices
    {
        private readonly IOrdenRepository _ordenRepository;
        private readonly IDetalleOrdenRepository _detalleOrdenRepository;
        private readonly IPagoRepository _pagoRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IMapperOrden _mapperOrden;
        private readonly IValidator<CreateOrdenDto> _createOrdenValidator;
        private readonly IValidator<CambiarEstadoOrdenDto> _cambiarEstadoValidator;
        private readonly IValidatorBusinessOrden _validatorBusinessOrden;
        private readonly INotificacionServices _notificacionServices;
        private readonly ILogger<OrdenServices> _logger;

        public OrdenServices(
            IOrdenRepository ordenRepository,
            IClienteRepository clienteRepository,
            IMapperOrden mapperOrden,
            IDetalleOrdenRepository detalleOrdenRepository,
            IPagoRepository pagoRepository,
            IValidator<CreateOrdenDto> createOrdenValidator,
            IValidator<CambiarEstadoOrdenDto> cambiarEstadoValidator,
            IValidatorBusinessOrden validatorBusinessOrden,
            INotificacionServices notificacionServices,
            ILogger<OrdenServices> logger)
        {
            _ordenRepository = ordenRepository;
            _clienteRepository = clienteRepository;
            _mapperOrden = mapperOrden;
            _detalleOrdenRepository = detalleOrdenRepository;
            _pagoRepository = pagoRepository;
            _createOrdenValidator = createOrdenValidator;
            _cambiarEstadoValidator = cambiarEstadoValidator;
            _validatorBusinessOrden = validatorBusinessOrden;
            _notificacionServices = notificacionServices;
            _logger = logger;
        }

        public async Task<OperationResultD<OrdenResponseDto>> CrearOrden(CreateOrdenDto dto, int usuarioId)
        {
            var dtoResult = await _createOrdenValidator.ValidateAsync(dto);
            if (!dtoResult.IsValid)
                return OperationResultD<OrdenResponseDto>.Failure(
                    string.Join(", ", dtoResult.Errors.Select(e => e.ErrorMessage)));

            var businessResult = await _validatorBusinessOrden.ValidarCrearOrdenAsync(dto.ClienteId);
            if (!businessResult.IsSuccess)
                return OperationResultD<OrdenResponseDto>.Failure(businessResult.Message);

            var orden = _mapperOrden.MapTOCreateOrden(dto, usuarioId);
            await _ordenRepository.CreateAsync(orden);

            var ordenConCliente = await _ordenRepository.GetByIdConClienteAsync(orden.Id);
            var ordenResponse = _mapperOrden.MapToOrdenResponseDto(ordenConCliente!);
            return OperationResultD<OrdenResponseDto>.Success(ordenResponse, "Orden creada exitosamente");
        }

        public async Task<OperationResultD<bool>> CambiarEstadoOrden(CambiarEstadoOrdenDto dto, int usuarioSolicitanteId, bool esAdministrador)
        {
            var dtoResult = await _cambiarEstadoValidator.ValidateAsync(dto);
            if (!dtoResult.IsValid)
                return OperationResultD<bool>.Failure(
                    string.Join(", ", dtoResult.Errors.Select(e => e.ErrorMessage)));

            var orden = await _ordenRepository.GetByIdAsync(dto.Id);
            if (orden == null)
                return OperationResultD<bool>.Failure("Orden no encontrada");

            if (!esAdministrador && orden.UsuarioId != usuarioSolicitanteId)
                return OperationResultD<bool>.Failure("No tiene permiso sobre esta orden");

            var nuevoEstado = Enum.Parse<EstadoOrden>(dto.NuevoEstado, true);

            var businessResult = await _validatorBusinessOrden.ValidarCambiarEstadoAsync(nuevoEstado);
            if (!businessResult.IsSuccess)
                return OperationResultD<bool>.Failure(businessResult.Message);

            // Esta es la segunda puerta a la cancelación: sin este guard, lo que CancelarOrden
            // bloquea se lograría igual mandando NuevoEstado = "Cancelada" por acá.
            var totalPagado = 0m;
            if (nuevoEstado == EstadoOrden.Cancelada)
            {
                var cancelacionResult = await ValidarCancelacionAsync(orden.Id, esAdministrador);
                if (!cancelacionResult.IsSuccess)
                    return OperationResultD<bool>.Failure(cancelacionResult.Message);

                totalPagado = cancelacionResult.Data;
            }

            orden.CambiarEstado(nuevoEstado);
            await _ordenRepository.UpdateAsync(orden);

            _logger.LogInformation("Orden {OrdenId} cambiada a estado {NuevoEstado} por el usuario {UsuarioSolicitanteId}", orden.Id, nuevoEstado, usuarioSolicitanteId);
            AuditarCancelacionConPagos(orden.Id, totalPagado, usuarioSolicitanteId);

            await _notificacionServices.NotificarUsuarioAsync(
                orden.UsuarioId,
                TipoNotificacion.CambioEstadoOrden,
                $"Tu orden #{orden.Id} pasó a estado {nuevoEstado}.");

            return OperationResultD<bool>.Success(true, "Estado de la orden actualizado exitosamente");
        }

        public async Task<OperationResultD<bool>> CancelarOrden(int id, int usuarioSolicitanteId, bool esAdministrador)
        {
            var orden = await _ordenRepository.GetByIdAsync(id);
            if (orden == null)
                return OperationResultD<bool>.Failure("Orden no encontrada");

            if (!esAdministrador && orden.UsuarioId != usuarioSolicitanteId)
                return OperationResultD<bool>.Failure("No tiene permiso sobre esta orden");

            var cancelacionResult = await ValidarCancelacionAsync(id, esAdministrador);
            if (!cancelacionResult.IsSuccess)
                return OperationResultD<bool>.Failure(cancelacionResult.Message);

            orden.CancelarOrden();
            await _ordenRepository.UpdateAsync(orden);

            _logger.LogInformation("Orden {OrdenId} cancelada por el usuario {UsuarioSolicitanteId}", id, usuarioSolicitanteId);
            AuditarCancelacionConPagos(id, cancelacionResult.Data, usuarioSolicitanteId);

            await _notificacionServices.NotificarUsuarioAsync(
                orden.UsuarioId,
                TipoNotificacion.CambioEstadoOrden,
                $"Tu orden #{orden.Id} pasó a estado {orden.Estado}.");

            return OperationResultD<bool>.Success(true, "Orden cancelada exitosamente");
        }

        public async Task<OperationResultD<bool>> ConfirmarOrden(int id, int usuarioSolicitanteId, bool esAdministrador)
        {
            var orden = await _ordenRepository.GetByIdAsync(id);
            if (orden == null)
                return OperationResultD<bool>.Failure("Orden no encontrada");

            if (!esAdministrador && orden.UsuarioId != usuarioSolicitanteId)
                return OperationResultD<bool>.Failure("No tiene permiso sobre esta orden");

            var detallesOrden = await _detalleOrdenRepository.ObtenerPorOrdenIdAsync(id);
            if (detallesOrden == null || detallesOrden.Count == 0 || orden.Total <= 0)
                return OperationResultD<bool>.Failure("No se puede confirmar una orden sin productos");

            orden.ConfirmarOrden();
            await _ordenRepository.UpdateAsync(orden);

            _logger.LogInformation("Orden {OrdenId} confirmada por el usuario {UsuarioSolicitanteId}", id, usuarioSolicitanteId);

            await _notificacionServices.NotificarUsuarioAsync(
                orden.UsuarioId,
                TipoNotificacion.CambioEstadoOrden,
                $"Tu orden #{orden.Id} pasó a estado {orden.Estado}.");

            return OperationResultD<bool>.Success(true, "Orden confirmada exitosamente");
        }

        public async Task<OperationResultD<List<OrdenResponseDto>>> ConsultarOrdenesPorCliente(int clienteId, EstadoOrden? estado = null)
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);
            if (cliente == null)
                return OperationResultD<List<OrdenResponseDto>>.Failure("Cliente no encontrado");

            var ordenes = await _ordenRepository.ObtenerPorClienteAsync(clienteId, estado);
            if (ordenes == null || ordenes.Count == 0)
                return OperationResultD<List<OrdenResponseDto>>.Success(new List<OrdenResponseDto>(), "El cliente no tiene órdenes");

            var ordenesResponse = ordenes.Select(o => _mapperOrden.MapToOrdenResponseDto(o)).ToList();
            return OperationResultD<List<OrdenResponseDto>>.Success(ordenesResponse, "Órdenes obtenidas exitosamente");
        }

        public async Task<OperationResultD<List<OrdenResponseDto>>> ConsultarOrdenesPorFecha(DateTime fecha, EstadoOrden? estado = null)
        {
            var ordenes = await _ordenRepository.ObtenerPorRangoFechaAsync(fecha.Date, fecha.Date.AddDays(1).AddTicks(-1), estado);
            if (ordenes.Count == 0)
                return OperationResultD<List<OrdenResponseDto>>.Success(new List<OrdenResponseDto>(), "No se encontraron órdenes para la fecha especificada");

            var ordenesResponse = ordenes.Select(o => _mapperOrden.MapToOrdenResponseDto(o)).ToList();
            return OperationResultD<List<OrdenResponseDto>>.Success(ordenesResponse, "Órdenes obtenidas exitosamente");
        }

        public async Task<OperationResultD<List<OrdenResponseDto>>> GetAllOrdenes(EstadoOrden? estado = null)
        {
            var ordenes = await _ordenRepository.GetAllConDetallesAsync(estado);
            if (ordenes.Count == 0)
                return OperationResultD<List<OrdenResponseDto>>.Success(new List<OrdenResponseDto>(), "No se encontraron órdenes");

            var ordenesResponse = ordenes.Select(o => _mapperOrden.MapToOrdenResponseDto(o)).ToList();
            return OperationResultD<List<OrdenResponseDto>>.Success(ordenesResponse, "Órdenes obtenidas exitosamente");
        }

        public async Task<OperationResultD<OrdenResponseDto>> GetOrdenByIdAsync(int id)
        {
            var orden = await _ordenRepository.GetByIdConClienteAsync(id);
            if (orden == null)
                return OperationResultD<OrdenResponseDto>.Failure("Orden no encontrada");

            var ordenResponse = _mapperOrden.MapToOrdenResponseDto(orden);
            return OperationResultD<OrdenResponseDto>.Success(ordenResponse, "Orden obtenida exitosamente");
        }

        public async Task<OperationResultD<List<OrdenResponseDto>>> GetOrdenesByUsuarioAsync(int usuarioId)
        {
            var ordenes = await _ordenRepository.ObtenerPorUsuarioAsync(usuarioId);
            if (ordenes == null || ordenes.Count == 0)
                return OperationResultD<List<OrdenResponseDto>>.Success(new List<OrdenResponseDto>(), "El usuario no tiene órdenes registradas");

            var ordenesResponse = ordenes.Select(o => _mapperOrden.MapToOrdenResponseDto(o)).ToList();
            return OperationResultD<List<OrdenResponseDto>>.Success(ordenesResponse, "Órdenes obtenidas exitosamente");
        }

        // CancelarOrden y CambiarEstadoOrden(Cancelada) son dos puertas a la misma operación,
        // así que la regla vive en un solo lugar. Devuelve el total ya cobrado para que quien
        // llama lo audite DESPUÉS de que la cancelación se haya persistido de verdad.
        private async Task<OperationResultD<decimal>> ValidarCancelacionAsync(int ordenId, bool esAdministrador)
        {
            var totalPagado = await _pagoRepository.ObtenerTotalPagadoPorOrdenAsync(ordenId);

            var businessResult = await _validatorBusinessOrden.ValidarCancelarOrdenAsync(totalPagado, esAdministrador);
            if (!businessResult.IsSuccess)
                return OperationResultD<decimal>.Failure(businessResult.Message);

            return OperationResultD<decimal>.Success(totalPagado);
        }

        // Cancelar una orden con dinero ya cobrado es la excepción que solo un administrador
        // puede autorizar: queda registrada junto al monto que se quedó sin contraparte.
        private void AuditarCancelacionConPagos(int ordenId, decimal totalPagado, int usuarioSolicitanteId)
        {
            if (totalPagado <= 0)
                return;

            _logger.LogWarning("Orden {OrdenId} cancelada con pagos registrados por {TotalPagado}, por el administrador {UsuarioSolicitanteId}", ordenId, totalPagado, usuarioSolicitanteId);
        }
    }
}
