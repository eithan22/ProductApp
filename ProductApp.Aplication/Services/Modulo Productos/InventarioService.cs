using FluentValidation;
using Microsoft.Extensions.Logging;
using ProductApp.Aplication.Common;
using ProductApp.Aplication.Dtos.Modulo_Productos.InventarioDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Interface.IMappers.Modulos_Productos;
using ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Producto;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Interfaces;

namespace ProductApp.Aplication.Services
{
    public class InventarioService : IInventarioServices
    {
        private readonly IInventarioRepository _inventarioRepository;
        private readonly IMapperInventario _mapperInventario;
        private readonly IValidator<MovimientoStockDto> _movimientoStockValidator;
        private readonly IValidator<AjustarStockDto> _ajustarStockValidator;
        private readonly IValidatorBusinessInventario _validatorBusinessInventario;
        private readonly ILogger<InventarioService> _logger;

        public InventarioService(
            IInventarioRepository inventarioRepository,
            IMapperInventario mapperInventario,
            IValidator<MovimientoStockDto> movimientoStockValidator,
            IValidator<AjustarStockDto> ajustarStockValidator,
            IValidatorBusinessInventario validatorBusinessInventario,
            ILogger<InventarioService> logger)
        {
            _inventarioRepository = inventarioRepository;
            _mapperInventario = mapperInventario;
            _movimientoStockValidator = movimientoStockValidator;
            _ajustarStockValidator = ajustarStockValidator;
            _validatorBusinessInventario = validatorBusinessInventario;
            _logger = logger;
        }

        public async Task<OperationResultD<InventarioResponseDto>> ObtenerInventarioAsync(int productoId)
        {
            var inventario = await _inventarioRepository.GetByProductoIdAsync(productoId);
            if (inventario == null)
                return OperationResultD<InventarioResponseDto>.Failure("Inventario no encontrado.");

            var response = _mapperInventario.MapToInventarioResponse(inventario);
            return OperationResultD<InventarioResponseDto>.Success(response, "Inventario obtenido exitosamente.");
        }

        public async Task<OperationResultD<InventarioResponseDto>> AgregarStockAsync(MovimientoStockDto dto)
        {
            var validationResult = await _movimientoStockValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return OperationResultD<InventarioResponseDto>.Failure(
                    $"Error de validación: {string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))}");

            var inventario = await _inventarioRepository.GetByProductoIdAsync(dto.ProductoId);
            if (inventario == null)
                return OperationResultD<InventarioResponseDto>.Failure("Inventario no encontrado.");

            var businessResult = await _validatorBusinessInventario.ValidarEntradaStockAsync(dto, inventario);
            if (!businessResult.IsSuccess)
                return OperationResultD<InventarioResponseDto>.Failure(businessResult.Message);

            inventario.RegistrarEntradaStock(dto.Cantidad);
            await _inventarioRepository.UpdateAsync(inventario);

            return OperationResultD<InventarioResponseDto>.Success(
                _mapperInventario.MapToInventarioResponse(inventario), "Stock agregado exitosamente.");
        }

        public async Task<OperationResultD<InventarioResponseDto>> DescontarStockAsync(MovimientoStockDto dto)
        {
            var validationResult = await _movimientoStockValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return OperationResultD<InventarioResponseDto>.Failure(
                    $"Error de validación: {string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))}");

            var inventario = await _inventarioRepository.GetByProductoIdAsync(dto.ProductoId);
            if (inventario == null)
                return OperationResultD<InventarioResponseDto>.Failure("Inventario no encontrado.");

            var businessResult = await _validatorBusinessInventario.ValidarSalidaStockAsync(dto, inventario);
            if (!businessResult.IsSuccess)
                return OperationResultD<InventarioResponseDto>.Failure(businessResult.Message);

            inventario.RegistrarSalidaStock(dto.Cantidad);
            await _inventarioRepository.UpdateAsync(inventario);

            return OperationResultD<InventarioResponseDto>.Success(
                _mapperInventario.MapToInventarioResponse(inventario), "Stock descontado exitosamente.");
        }

        public async Task<OperationResultD<InventarioResponseDto>> AjustarStockAsync(AjustarStockDto dto)
        {
            var validationResult = await _ajustarStockValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return OperationResultD<InventarioResponseDto>.Failure(
                    $"Error de validación: {string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))}");

            var inventario = await _inventarioRepository.GetByProductoIdAsync(dto.ProductoId);
            if (inventario == null)
                return OperationResultD<InventarioResponseDto>.Failure("Inventario no encontrado.");

            var businessResult = await _validatorBusinessInventario.ValidarAjusteStockAsync(dto, inventario);
            if (!businessResult.IsSuccess)
                return OperationResultD<InventarioResponseDto>.Failure(businessResult.Message);

            inventario.AjustarStock(dto.NuevoStock);

            if (dto.NuevoStockMinimo.HasValue)
                inventario.AjustarStockMinimo(dto.NuevoStockMinimo.Value);

            await _inventarioRepository.UpdateAsync(inventario);

            _logger.LogInformation("Ajuste manual de inventario para el producto {ProductoId}: nuevo stock {NuevoStock}", dto.ProductoId, dto.NuevoStock);

            return OperationResultD<InventarioResponseDto>.Success(
                _mapperInventario.MapToInventarioResponse(inventario), "Stock ajustado exitosamente.");
        }

        public async Task<OperationResultD<List<InventarioResponseDto>>> ObtenerStockBajoAsync()
        {
            var inventarios = await _inventarioRepository.GetStockBajoAsync();
            if (inventarios == null)
                return OperationResultD<List<InventarioResponseDto>>.Failure("No se encontraron inventarios con stock bajo.");

            var response = inventarios.Select(i => _mapperInventario.MapToInventarioResponse(i)).ToList();
            return OperationResultD<List<InventarioResponseDto>>.Success(response, "Inventarios con stock bajo obtenidos exitosamente.");
        }

        public async Task<OperationResultD<PagedResult<InventarioResponseDto>>> ObtenerTodosInventariosAsync(int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1)
                return OperationResultD<PagedResult<InventarioResponseDto>>.Failure("pageNumber debe ser mayor o igual a 1");

            if (pageSize < 1 || pageSize > 100)
                return OperationResultD<PagedResult<InventarioResponseDto>>.Failure("pageSize debe estar entre 1 y 100");

            var (inventarios, totalCount) = await _inventarioRepository.GetAllConProductoAsync(pageNumber, pageSize);

            var response = inventarios.Select(i => _mapperInventario.MapToInventarioResponse(i)).ToList();

            var pagedResult = new PagedResult<InventarioResponseDto>
            {
                Items = response,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return OperationResultD<PagedResult<InventarioResponseDto>>.Success(pagedResult, "Todos los inventarios obtenidos exitosamente.");
        }
    }
}
