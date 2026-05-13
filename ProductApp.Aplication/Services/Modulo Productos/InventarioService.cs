using FluentValidation;
using Microsoft.Win32;
using ProductApp.Aplication.Dtos.CategoriaDto;
using ProductApp.Aplication.Dtos.Modulo_Productos.InventarioDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Interface.IMappers.Modulos_Productos;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Text;

namespace ProductApp.Aplication.Services
{
    public class InventarioService : IInventarioServices
    {
        private readonly IProductoServices _productoServices;
        private readonly IInventarioRepository _inventarioRepository;
        public readonly IMapperInventario _mapperInventario;
        public readonly IValidator<MovimientoStockDto> _movimientoStockValidator;
        public readonly IValidator<AjustarStockDto> _ajustarStockValidator;

        public InventarioService(IProductoServices productoServices,
            IInventarioRepository inventarioRepository,
            IMapperInventario mapperInventario,
           IValidator<MovimientoStockDto> movimientoStockValidator,
              IValidator<AjustarStockDto> ajustarStockValidator

            )
        {
            _productoServices = productoServices;
            _inventarioRepository = inventarioRepository;
            _mapperInventario = mapperInventario;
            _movimientoStockValidator = movimientoStockValidator;
            _ajustarStockValidator = ajustarStockValidator;
        }

        /*

        Mostrar stock actual de cada producto.
       • Descontar automáticamente el stock al confirmar una orden pagada.
       • Mostrar alertas cuando el stock sea menor al mínimo permitido.
       • Registrar la fecha de última actualización.
        • Permitir ajustes manuales de inventario por parte del Administrador.

        */



        public async Task<OperationResultD<InventarioResponseDto>> ObtenerInventarioAsync(int productoId)
        {
            var inventario = await _inventarioRepository.GetByProductoIdAsync(productoId);

            if (inventario == null)
            {
                return OperationResultD<InventarioResponseDto>.Failure("Inventario no encontrado.");
            }

            var response = _mapperInventario.MapToInventarioResponse(inventario);

            return OperationResultD<InventarioResponseDto>.Success(response, "Inventario obtenido exitosamente.");
        }




        public async Task<OperationResultD<InventarioResponseDto>> AgregarStockAsync(MovimientoStockDto agregarStockDto)
        {
            var validationResult = await _movimientoStockValidator.ValidateAsync(agregarStockDto);

            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return OperationResultD<InventarioResponseDto>.Failure($"Error de validación: {errors}");

            }


            var producto = await _productoServices.GetByIdAsync(agregarStockDto.ProductoId);

            if (!producto.IsSuccess)
            {
                return OperationResultD<InventarioResponseDto>.Failure("Producto no encontrado.");
            }

            // Buscar inventario del producto
            var inventario = await _inventarioRepository.GetByProductoIdAsync(agregarStockDto.ProductoId);

            if (inventario == null)
            {
                return OperationResultD<InventarioResponseDto>.Failure("Inventario no encontrado.");
            }
            // Agregar stock
            inventario.RegistrarEntradaStock(agregarStockDto.cantidad);

            await _inventarioRepository.UpdateAsync(inventario);

            // Mapear a DTO de respuesta
            var response = _mapperInventario.MapToInventarioResponse(inventario);

            return OperationResultD<InventarioResponseDto>.Success(response, "Stock agregado exitosamente.");
        }



        public async Task<OperationResultD<InventarioResponseDto>> DescontarStockAsync(MovimientoStockDto descontarStockDto)
        {
            var validationResult = await _movimientoStockValidator.ValidateAsync(descontarStockDto);

            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return OperationResultD<InventarioResponseDto>.Failure($"Error de validación: {errors}");

            }

            var producto = await _productoServices.GetByIdAsync(descontarStockDto.ProductoId);

            if (!producto.IsSuccess)
            {
                return OperationResultD<InventarioResponseDto>.Failure("Producto no encontrado.");
            }

            // Buscar inventario del producto
            var inventario = await _inventarioRepository.GetByProductoIdAsync(descontarStockDto.ProductoId);

            if (inventario == null)
            {
                return OperationResultD<InventarioResponseDto>.Failure("Inventario no encontrado.");
            }
            // Descontar stock
            inventario.RegistrarSalidaStock(descontarStockDto.cantidad);

            await _inventarioRepository.UpdateAsync(inventario);

            // Mapear a DTO de respuesta
            var response = _mapperInventario.MapToInventarioResponse(inventario);

            return OperationResultD<InventarioResponseDto>.Success(response, "Stock descontado exitosamente.");
        }







        // Este método se puede usar para ajustes manuales de stock, como correcciones o auditorías
        public async Task<OperationResultD<InventarioResponseDto>> AjustarStockAsync(AjustarStockDto ajustarStockDto)
        {
            var validationResult = await _ajustarStockValidator.ValidateAsync(ajustarStockDto);

            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return OperationResultD<InventarioResponseDto>.Failure($"Error de validación: {errors}");

            }


            var producto = await _productoServices.GetByIdAsync(ajustarStockDto.ProductoId);

            if (!producto.IsSuccess)
            {
                return OperationResultD<InventarioResponseDto>.Failure("Producto no encontrado.");
            }

            // Buscar inventario del producto
            var inventario = await _inventarioRepository.GetByProductoIdAsync(ajustarStockDto.ProductoId);

            if (inventario == null)
            {
                return OperationResultD<InventarioResponseDto>.Failure("Inventario no encontrado.");
            }
            // Ajustar stock
            inventario.AjustarStock(ajustarStockDto.NuevoStock);

            await _inventarioRepository.UpdateAsync(inventario);

            // Mapear a DTO de respuesta
            var response = _mapperInventario.MapToInventarioResponse(inventario);

            return OperationResultD<InventarioResponseDto>.Success(response, "Stock ajustado exitosamente.");


        }





        public async Task<OperationResultD<List<InventarioResponseDto>>> ObtenerStockBajoAsync()
        {
            var inventarios = await _inventarioRepository.GetStockBajoAsync();

            if (inventarios == null) 
            {
                return OperationResultD<List<InventarioResponseDto>>.Failure("No se encontraron inventarios con stock bajo.");
            }

            var response = inventarios.Select(i => _mapperInventario.MapToInventarioResponse(i)).ToList();

            return OperationResultD<List<InventarioResponseDto>>.Success(response, "Inventarios con stock bajo obtenidos exitosamente.");
        }



        public async Task<OperationResultD<List<InventarioResponseDto>>> ObtenerTodosInventariosAsync()
        {
            var inventarios = await _inventarioRepository.GetAllInventariosAsync();

            if (inventarios == null || !inventarios.Any())
            {
                return OperationResultD<List<InventarioResponseDto>>.Failure("No se encontraron inventarios.");
            }


            var response = inventarios.Select(i => _mapperInventario.MapToInventarioResponse(i)).ToList();

            return OperationResultD<List<InventarioResponseDto>>.Success(response, "Todos los inventarios obtenidos exitosamente.");
        }
    }
}