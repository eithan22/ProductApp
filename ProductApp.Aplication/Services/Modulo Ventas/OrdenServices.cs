using FluentValidation;
using Microsoft.Extensions.Logging;
using ProductApp.Aplication.Dtos.Modulo_Ventas.OrdenDto;
using ProductApp.Aplication.Dtos.OrdenDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Interface.IMappers.Modulo_Ventas;
using ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Ventas;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Common.Enums.EnumsOrden;
using ProductApp.Domian.Interfaces;

namespace ProductApp.Aplication.Services
{
    public class OrdenServices : IOrdenServices
    {
        private readonly IOrdenRepository _ordenRepository;
        private readonly IDetalleOrdenRepository _detalleOrdenRepository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IMapperOrden _mapperOrden;
        private readonly IValidator<CreateOrdenDto> _createOrdenValidator;
        private readonly IValidator<CambiarEstadoOrdenDto> _cambiarEstadoValidator;
        private readonly IValidatorBusinessOrden _validatorBusinessOrden;
        private readonly ILogger<OrdenServices> _logger;

        public OrdenServices(
            IOrdenRepository ordenRepository,
            IClienteRepository clienteRepository,
            IMapperOrden mapperOrden,
            IDetalleOrdenRepository detalleOrdenRepository,
            IValidator<CreateOrdenDto> createOrdenValidator,
            IValidator<CambiarEstadoOrdenDto> cambiarEstadoValidator,
            IValidatorBusinessOrden validatorBusinessOrden,
            ILogger<OrdenServices> logger)
        {
            _ordenRepository = ordenRepository;
            _clienteRepository = clienteRepository;
            _mapperOrden = mapperOrden;
            _detalleOrdenRepository = detalleOrdenRepository;
            _createOrdenValidator = createOrdenValidator;
            _cambiarEstadoValidator = cambiarEstadoValidator;
            _validatorBusinessOrden = validatorBusinessOrden;
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

        public async Task<OperationResultD<bool>> CambiarEstadoOrden(CambiarEstadoOrdenDto dto)
        {
            var dtoResult = await _cambiarEstadoValidator.ValidateAsync(dto);
            if (!dtoResult.IsValid)
                return OperationResultD<bool>.Failure(
                    string.Join(", ", dtoResult.Errors.Select(e => e.ErrorMessage)));

            var orden = await _ordenRepository.GetByIdAsync(dto.Id);
            if (orden == null)
                return OperationResultD<bool>.Failure("Orden no encontrada");

            var nuevoEstado = Enum.Parse<EstadoOrden>(dto.NuevoEstado, true);
            orden.CambiarEstado(nuevoEstado);
            await _ordenRepository.UpdateAsync(orden);

            return OperationResultD<bool>.Success(true, "Estado de la orden actualizado exitosamente");
        }

        public async Task<OperationResultD<bool>> CancelarOrden(int id, int usuarioSolicitanteId)
        {
            var orden = await _ordenRepository.GetByIdAsync(id);
            if (orden == null)
                return OperationResultD<bool>.Failure("Orden no encontrada");

            orden.CancelarOrden();
            await _ordenRepository.UpdateAsync(orden);

            _logger.LogInformation("Orden {OrdenId} cancelada por el usuario {UsuarioSolicitanteId}", id, usuarioSolicitanteId);

            return OperationResultD<bool>.Success(true, "Orden cancelada exitosamente");
        }

        public async Task<OperationResultD<List<OrdenResponseDto>>> ConsultarOrdenesPorCliente(int clienteId)
        {
            var cliente = await _clienteRepository.GetByIdAsync(clienteId);
            if (cliente == null)
                return OperationResultD<List<OrdenResponseDto>>.Failure("Cliente no encontrado");

            var ordenes = await _ordenRepository.ObtenerPorClienteAsync(clienteId);
            if (ordenes == null || ordenes.Count == 0)
                return OperationResultD<List<OrdenResponseDto>>.Success(new List<OrdenResponseDto>(), "El cliente no tiene órdenes");

            var ordenesResponse = ordenes.Select(o => _mapperOrden.MapToOrdenResponseDto(o)).ToList();
            return OperationResultD<List<OrdenResponseDto>>.Success(ordenesResponse, "Órdenes obtenidas exitosamente");
        }

        public async Task<OperationResultD<List<OrdenResponseDto>>> ConsultarOrdenesPorFecha(DateTime fecha)
        {
            var ordenes = await _ordenRepository.ObtenerPorRangoFechaAsync(fecha.Date, fecha.Date.AddDays(1).AddTicks(-1));
            if (ordenes.Count == 0)
                return OperationResultD<List<OrdenResponseDto>>.Success(new List<OrdenResponseDto>(), "No se encontraron órdenes para la fecha especificada");

            var ordenesResponse = ordenes.Select(o => _mapperOrden.MapToOrdenResponseDto(o)).ToList();
            return OperationResultD<List<OrdenResponseDto>>.Success(ordenesResponse, "Órdenes obtenidas exitosamente");
        }

        public async Task<OperationResultD<List<OrdenResponseDto>>> GetAllOrdenes()
        {
            var ordenes = await _ordenRepository.GetAllConDetallesAsync();
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

        public async Task<OperationResultD<bool>> RecalcularTotalAsync(int id)
        {
            var orden = await _ordenRepository.GetByIdAsync(id);
            if (orden == null)
                return OperationResultD<bool>.Failure("Orden no encontrada");

            var detallesOrden = await _detalleOrdenRepository.ObtenerPorOrdenIdAsync(id);
            if (detallesOrden == null || detallesOrden.Count == 0)
                return OperationResultD<bool>.Failure("No se encontraron detalles para la orden");

            orden.ActualizarTotal(detallesOrden.Sum(d => d.Subtotal));
            await _ordenRepository.UpdateAsync(orden);

            return OperationResultD<bool>.Success(true, "Total de la orden recalculado exitosamente");
        }
    }
}
