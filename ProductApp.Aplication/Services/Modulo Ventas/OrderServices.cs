using FluentValidation;
using ProductApp.Aplication.Dtos.Modulo_Ventas.OrdenDto;
using ProductApp.Aplication.Dtos.OrdenDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Interface.IMappers.Modulo_Ventas;
using ProductApp.Aplication.Interface.Servicios.BaseServices;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Common.Enums.EnumsOrden;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Text;

namespace ProductApp.Aplication.Services
{
    public class OrderServices : IOrdenServices
    {
        private readonly IOrdenRepository _ordenRepository;
        private readonly IDetalleOrdenRepository _detalleOrdenRepository;
        private readonly IClienteServices _clienteServices;
        private readonly IMapperOrden _mapperOrden;
        private readonly IValidator<CreateOrdenDto> _createOrdenValidator;
        
        public OrderServices(IOrdenRepository ordenRepository,
            IClienteServices clienteServices,
            IMapperOrden mapperOrden,
            IDetalleOrdenRepository detalleOrdenRepository,
            IValidator<CreateOrdenDto> createOrdenValidator
            )


        {
            _ordenRepository = ordenRepository;
            _clienteServices = clienteServices;
            _mapperOrden = mapperOrden;
            _detalleOrdenRepository = detalleOrdenRepository;
            _createOrdenValidator = createOrdenValidator;
        }

        // Cambiar el estado de una orden (Pendiente, Pagada, Cancelada, Entregada).
        public async Task<OperationResultD<bool>> CambiarEstadoOrden(CambiarEstadoOrdenDto dto)
        {
            var orden = await _ordenRepository.GetByIdAsync(dto.Id);
            if (orden == null)
            {
                return OperationResultD<bool>.Failure("Orden no encontrada");
            }

            orden.Estado = dto.NuevoEstado;

            await _ordenRepository.UpdateAsync(orden);

            return OperationResultD<bool>.Success(true, "Estado de la orden actualizado exitosamente");
        }



        // Cancelar una orden antes de ser pagada.
        public async Task<OperationResultD<bool>> CancelarOrden(int id)
        {
            var orden = await _ordenRepository.GetByIdAsync(id);
            if(orden == null)
            {
                return OperationResultD<bool>.Failure("Orden no encontrada");
            }

            if(orden.Estado != EstadoOrden.Pendiente)
            {
                return OperationResultD<bool>.Failure("Solo se pueden cancelar órdenes pendientes");
            }

            orden.Estado = EstadoOrden.Cancelada;
            await _ordenRepository.UpdateAsync(orden);

            return OperationResultD<bool>.Success(true, "Orden cancelada exitosamente");
        }



        // Consultar órdenes por cliente.

        public async Task<OperationResultD<List<OrdenResponseDto>>> ConsultarOrdenesPorCliente(int clienteId)
        {
            var cliente = await _clienteServices.GetByIdAsync(clienteId);
            if (cliente == null)
            {
                return OperationResultD<List<OrdenResponseDto>>.Failure("Cliente no encontrado");
            }
            // Obtener las órdenes del cliente
            var ordenes = await _ordenRepository.ObtenerPorClienteAsync(clienteId);

            if (ordenes == null)
            {
                return OperationResultD<List<OrdenResponseDto>>.Failure("No se encontraron órdenes para el cliente");
            }

            // Mapear las órdenes a DTOs de respuesta
            var ordenesResponse = ordenes.Select(o => _mapperOrden.MapToOrdenResponseDto(o)).ToList();
            


            return OperationResultD<List<OrdenResponseDto>>.Success(ordenesResponse, "Órdenes obtenidas exitosamente");
        }






        // Consultar órdenes por fecha.
        public async Task<OperationResultD<List<OrdenResponseDto>>> ConsultarOrdenesPorFecha(DateTime fecha)
        {

            // Obtener las órdenes por fecha
            var ordenes = await _ordenRepository.ObtenerPorFechaAsync(fecha);

            if(ordenes == null)
            {
                return OperationResultD<List<OrdenResponseDto>>.Failure("No se encontraron órdenes para la fecha especificada");
            }

            var ordenesResponse = ordenes.Select(o => _mapperOrden.MapToOrdenResponseDto(o)).ToList();

            return OperationResultD<List<OrdenResponseDto>>.Success(ordenesResponse, "Órdenes obtenidas exitosamente");
        }



        // Crear una nueva orden.

        public async Task<OperationResultD<OrdenResponseDto>> CrearOrden(CreateOrdenDto dto)
        {
            var  cliente = await _clienteServices.GetByIdAsync(dto.ClienteId);

            if(cliente == null)
            {
                return OperationResultD<OrdenResponseDto>.Failure("Cliente no encontrado");
            }

            var orden =  _mapperOrden.MapTOCreateOrden(dto);

            await _ordenRepository.CreateAsync(orden);

            var ordenResponse = _mapperOrden.MapToOrdenResponseDto(orden);

            return OperationResultD<OrdenResponseDto>.Success(ordenResponse, "Orden creada exitosamente");


        }

        public Task<OperationResultD<List<OrdenResponseDto>>> GetAllOrdenes()
        {
            var ordenes = _ordenRepository.GetAllAsync();

            if(ordenes == null)
            {
                return Task.FromResult(OperationResultD<List<OrdenResponseDto>>.Failure("No se encontraron órdenes"));
            }

            var ordenesResponse = ordenes.Result.Select(o => _mapperOrden.MapToOrdenResponseDto(o)).ToList();

            return Task.FromResult(OperationResultD<List<OrdenResponseDto>>.Success(ordenesResponse, "Órdenes obtenidas exitosamente"));
        }






        // Recalcular el total de una orden después de agregar o actualizar los detalles de la orden.

        public async Task<OperationResultD<bool>> RecalcularTotalAsync(int id)
        {
            // Obtener la orden por su ID
            var orden = await _ordenRepository.GetByIdAsync(id);
            if (orden == null)
            {
                return OperationResultD<bool>.Failure("Orden no encontrada");
            }

            var detallesOrden = await _detalleOrdenRepository.ObtenerDetalleOrdenPorOrdenIdAsync(id);

            if (detallesOrden == null || detallesOrden.Count == 0)
            {
                return OperationResultD<bool>.Failure("No se encontraron detalles para la orden");
            }

            orden.Total = detallesOrden.Sum(d => d.Subtotal);

            // Lógica para recalcular el total de la orden

            await _ordenRepository.UpdateAsync(orden);

            return OperationResultD<bool>.Success(true, "Total de la orden recalculado exitosamente");
        }




        /*
        El sistema debe permitir:
       • Crear una nueva orden.
       • Asociar la orden a un cliente.
       • Agregar múltiples productos a la orden.
       • Calcular automáticamente el subtotal por producto.
       • Calcular automáticamente el total general.
       • Cambiar el estado de la orden (Pendiente, Pagada, Cancelada, Entregada).
       • Cancelar órdenes antes de ser pagadas.
       • Consultar órdenes por fecha o cliente.

        */



    }

}
