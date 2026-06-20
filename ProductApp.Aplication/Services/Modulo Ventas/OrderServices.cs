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

            orden.CambiarEstado(dto.NuevoEstado);

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

            orden.CancelarOrden();

            await _ordenRepository.UpdateAsync(orden);

            return OperationResultD<bool>.Success(true, "Orden cancelada exitosamente");
        }



        // Consultar órdenes por cliente.

        public async Task<OperationResultD<List<OrdenResponseDto>>> ConsultarOrdenesPorCliente(int clienteId)
        {
            var cliente = await _clienteServices.GetByIdAsync(clienteId);

            // Verificar si el cliente existe
            if (!cliente.IsSuccess)
            {
                return OperationResultD<List<OrdenResponseDto>>.Failure(cliente.Message);
            }


            // Obtener las órdenes del cliente
            var ordenes = await _ordenRepository.ObtenerPorClienteAsync(clienteId);


            //esto no funciona ya que si evia una lista vacia si un cloente no tiene una orden 
            if (ordenes == null)
            {
                return OperationResultD<List<OrdenResponseDto>>.Failure("No se encontraron órdenes para el cliente");
            }

            if(ordenes.Count == 0)
            {
                return OperationResultD<List<OrdenResponseDto>>.Failure("El cliente no tiene órdenes");
            }

            // Mapear las órdenes a DTOs de respuesta
            var ordenesResponse = ordenes.Select(o => _mapperOrden.MapToOrdenResponseDto(o)).ToList();
            


            return OperationResultD<List<OrdenResponseDto>>.Success(ordenesResponse, "Órdenes obtenidas exitosamente");
        }






        // Consultar órdenes por fecha.
        public async Task<OperationResultD<List<OrdenResponseDto>>> ConsultarOrdenesPorFecha(DateTime fecha)
        {

            // Obtener las órdenes por fecha
            var ordenes = await _ordenRepository.ObtenerPorRangoFechaAsync(fecha.Date, fecha.Date.AddDays(1).AddTicks(-1));


            if(ordenes.Count == 0)
            {
                return OperationResultD<List<OrdenResponseDto>>.Failure("No se encontraron órdenes para la fecha especificada");
            }
             //no sirbe ya que una va a mandar una lista vacia y no es null
             /*
            if(ordenes == null)
            {
                return OperationResultD<List<OrdenResponseDto>>.Failure("No se encontraron órdenes para la fecha especificada");
            }
             */

            var ordenesResponse = ordenes.Select(o => _mapperOrden.MapToOrdenResponseDto(o)).ToList();

            return OperationResultD<List<OrdenResponseDto>>.Success(ordenesResponse, "Órdenes obtenidas exitosamente");
        }



        // Crear una nueva orden.

        public async Task<OperationResultD<OrdenResponseDto>> CrearOrden(CreateOrdenDto dto, int usuarioId)
        {
            

            var  cliente = await _clienteServices.GetByIdAsync(dto.ClienteId);

            if(!cliente.IsSuccess)
            {
                return OperationResultD<OrdenResponseDto>.Failure(cliente.Message);
            }

            // Inicializar la orden con valores predeterminados 
           var  orden = _mapperOrden.MapTOCreateOrden(dto, usuarioId);


            await _ordenRepository.CreateAsync(orden);

            var ordenResponse = _mapperOrden.MapToOrdenResponseDto(orden);

            return OperationResultD<OrdenResponseDto>.Success(ordenResponse, "Orden creada exitosamente");


        }

        public async Task<OperationResultD<List<OrdenResponseDto>>> GetAllOrdenes()
        {
            var ordenes = await _ordenRepository.GetAllConDetallesAsync();

            //LO MISMO 
            /*
            if(ordenes == null)
            {
                return OperationResultD<List<OrdenResponseDto>>.Failure("Ordenes no encontradas");
            }
            */
           

            if(ordenes.Count == 0)
            {
                return OperationResultD<List<OrdenResponseDto>>.Failure("No se encontraron órdenes");
            }

            var ordenesResponse = ordenes.Select(o => _mapperOrden.MapToOrdenResponseDto(o)).ToList();

            return(OperationResultD<List<OrdenResponseDto>>.Success(ordenesResponse, "Órdenes obtenidas exitosamente"));       
        
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

            var detallesOrden = await _detalleOrdenRepository.ObtenerPorOrdenIdAsync(id);

            if (detallesOrden == null || detallesOrden.Count == 0)
            {
                return OperationResultD<bool>.Failure("No se encontraron detalles para la orden");
            }

            orden.ActualizarTotal(detallesOrden.Sum(d => d.Subtotal));

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
