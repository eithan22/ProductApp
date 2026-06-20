using FluentValidation;
using ProductApp.Aplication.Dtos.PagoDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Interface.IMappers.Modulo_Ventas;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Common.Enums.EnumsOrden;
using ProductApp.Domian.Common.Enums.EnumsPago;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public PagoService(
            IPagoRepository pagoRepository,
            IOrdenRepository ordenRepository,
            IDetalleOrdenRepository detalleOrdenRepository,
            IInventarioRepository inventarioRepository,
            IMapperPago mapperPago,
            IValidator<CreatePagoDto> createPagoValidator)
        {
            _pagoRepository           = pagoRepository;
            _ordenRepository          = ordenRepository;
            _detalleOrdenRepository   = detalleOrdenRepository;
            _inventarioRepository     = inventarioRepository;
            _mapperPago               = mapperPago;
            _createPagoValidator      = createPagoValidator;
        }

        public async Task<OperationResultD<PagoResponseDto>> RegistrarPagoAsync(CreatePagoDto dto)
        {
            var validationResult = await _createPagoValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return OperationResultD<PagoResponseDto>.Failure(
                    string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

            var orden = await _ordenRepository.GetByIdAsync(dto.OrdenId);
            if (orden == null)
                return OperationResultD<PagoResponseDto>.Failure("Orden no encontrada");

            if (orden.Estado != EstadoOrden.Pendiente)
                return OperationResultD<PagoResponseDto>.Failure(
                    "Solo se pueden registrar pagos para órdenes en estado Pendiente");

            if (orden.Total <= 0)
                return OperationResultD<PagoResponseDto>.Failure(
                    "La orden no tiene total calculado. Agregue productos antes de registrar un pago");

            var totalPagado   = await _pagoRepository.ObtenerTotalPagadoPorOrdenAsync(dto.OrdenId);
            var saldoActual   = orden.Total - totalPagado;

            if (dto.Monto > saldoActual)
                return OperationResultD<PagoResponseDto>.Failure(
                    $"El monto ({dto.Monto}) supera el saldo pendiente ({saldoActual})");

            var nuevoSaldo   = saldoActual - dto.Monto;
            var pagoCompleto = nuevoSaldo <= 0;

            // Si este pago completa la orden, validar todo el inventario ANTES de guardar nada
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

            // Guardar el pago
            var pago = _mapperPago.MapToCreatePago(dto);
            await _pagoRepository.CreateAsync(pago);

            // Si el pago es completo: marcar pago, cambiar estado de orden y descontar inventario
            if (pagoCompleto)
            {
                pago.MarcarComoCompletado();
                await _pagoRepository.UpdateAsync(pago);

                orden.CambiarEstado(EstadoOrden.Pagada);
                await _ordenRepository.UpdateAsync(orden);

                foreach (var (inventario, cantidad) in inventariosADescontar)
                {
                    inventario.RegistrarSalidaStock(cantidad);
                    await _inventarioRepository.UpdateAsync(inventario);
                }
            }

            var mensaje = pagoCompleto
                ? "Pago registrado. La orden ha sido marcada como Pagada e inventario descontado"
                : $"Pago parcial registrado exitosamente. Saldo pendiente: {nuevoSaldo}";

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
                return OperationResultD<List<PagoResponseDto>>.Failure(
                    "No se encontraron pagos para esta orden");

            var totalPagado     = pagos.Sum(p => p.Monto);
            var saldoPendiente  = orden.Total - totalPagado;

            var response = pagos
                .Select(p => _mapperPago.MapToPagoResponseDto(p, saldoPendiente))
                .ToList();

            return OperationResultD<List<PagoResponseDto>>.Success(response, "Pagos obtenidos exitosamente");
        }




        public async Task<OperationResultD<decimal>> ObtenerSaldoPendienteAsync(int ordenId)
        {
            var orden = await _ordenRepository.GetByIdAsync(ordenId);
            if (orden == null)
                return OperationResultD<decimal>.Failure("Orden no encontrada");

            var totalPagado    = await _pagoRepository.ObtenerTotalPagadoPorOrdenAsync(ordenId);
            var saldoPendiente = orden.Total - totalPagado;

            return OperationResultD<decimal>.Success(saldoPendiente, "Saldo pendiente obtenido exitosamente");
        }
    }
}
