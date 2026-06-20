using FluentValidation;
using ProductApp.Aplication.Dtos.Modulo_Ventas.DetalleOrdenDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Interface.IMappers.Modulo_Ventas;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Common.Enums.EnumsOrden;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ProductApp.Aplication.Services
{
    public class DetalleOrdenService : IDetalleOrdenServices
    {
        private readonly IDetalleOrdenRepository _detalleOrdenRepository;
        private readonly IOrdenServices _ordenServices;
        private readonly IOrdenRepository _ordenRepository;
        private readonly IProductoRepository _productoRepository;
        private readonly IMapperDetalleOrden _mapperDetalleOrdencs;
        private readonly IValidator<CreateDetalleOrdenDto> _createDetalleOrdenValidator;
        private readonly IValidator<UpdateDetalleOrdenDto> _updateDetalleOrdenValidator;

        public DetalleOrdenService(IDetalleOrdenRepository detalleOrdenRepository,
            IOrdenServices ordenServices,
            IOrdenRepository ordenRepository,
            IProductoRepository productoRepository,
            IMapperDetalleOrden mapperDetalleOrdencs,
            IValidator<CreateDetalleOrdenDto> createDetalleOrdenValidator,
            IValidator<UpdateDetalleOrdenDto> updateDetalleOrdenValidator)
        {
            _detalleOrdenRepository = detalleOrdenRepository;
            _ordenServices = ordenServices;
            _ordenRepository = ordenRepository;
            _productoRepository = productoRepository;
            _mapperDetalleOrdencs = mapperDetalleOrdencs;
            _createDetalleOrdenValidator = createDetalleOrdenValidator;
            _updateDetalleOrdenValidator = updateDetalleOrdenValidator;
        }

        // Implementación del método para actualizar la cantidad de un producto en el detalle de la orden
        public async Task<OperationResultD<OrdenDetalleResponseDto>> ActualizarDetalleOrden(int id, UpdateDetalleOrdenDto dto)
        {
            var validatorDto = await _updateDetalleOrdenValidator.ValidateAsync(dto);

           
            if (!validatorDto.IsValid)
            {
                return OperationResultD<OrdenDetalleResponseDto>.Failure("Error al validar los datos de entrada: " + string.Join(", ", validatorDto.Errors.Select(e => e.ErrorMessage)));
            }



            var detalleOrden = await _detalleOrdenRepository.GetByIdAsync(id);
            if (detalleOrden == null) 
            {
                return OperationResultD<OrdenDetalleResponseDto>.Failure("Detalle de orden no encontrado");

            }

            var orden = await _ordenRepository.GetByIdAsync(detalleOrden.OrdenId);

            if(orden == null)
            {
                return OperationResultD<OrdenDetalleResponseDto>.Failure("Orden no encontrada");
            }

            if(orden.Estado != EstadoOrden.Pendiente)
            {
                return OperationResultD<OrdenDetalleResponseDto>.Failure("No se pueden modificar detalles de una orden que no está pendiente");
            }

            var producto = await _productoRepository.ObtenerConInventarioAsync(detalleOrden.ProductId);

            if(producto == null)
            {
                return OperationResultD<OrdenDetalleResponseDto>.Failure("Producto no encontrado");
            }

            // Validar que el producto tenga inventario asociado
            if (producto.Inventario == null)
            {
                return OperationResultD<OrdenDetalleResponseDto>.Failure("Inventario no encontrado para este producto");
            }
            // Validar que la cantidad solicitada no exceda el stock disponible
            if (dto.Cantidad > producto.Inventario.CantidadActual)
            {
                return OperationResultD<OrdenDetalleResponseDto>.Failure("Cantidad solicitada excede el stock disponible");
            }

            // Actualizar la cantidad y recalcular el subtotal

            _mapperDetalleOrdencs.MapToUpdateDetalleOrden(dto, detalleOrden);

            // Guardar los cambios en el repositorio

            await _detalleOrdenRepository.UpdateAsync(detalleOrden);


            // Recalcular el total de la orden después de actualizar el detalle

            var resultadoRecalculo = await _ordenServices.RecalcularTotalAsync(detalleOrden.OrdenId);

            if (!resultadoRecalculo.IsSuccess)
            {
                return OperationResultD<OrdenDetalleResponseDto>.Failure("Error al recalcular el total de la orden: " + resultadoRecalculo.Message);
            }

            // Obtener el detalle de orden actualizado con la información del producto para la respuesta
            var detalleConProducto = await _detalleOrdenRepository.ObtenerConProductoAsync(detalleOrden.Id);

            if (detalleConProducto == null)
            {
                return OperationResultD<OrdenDetalleResponseDto>.Failure("Error al obtener el detalle de orden con el producto");
            }


            var detalleOrdenResponse = _mapperDetalleOrdencs.MapToDetalleOrdenResponseDto(detalleConProducto);

            return OperationResultD<OrdenDetalleResponseDto>.Success(detalleOrdenResponse, "Detalle de orden actualizado exitosamente");


        }




        // Implementación del método para agregar un producto al detalle de la orden
        public async Task<OperationResultD<OrdenDetalleResponseDto>> AgregarProductoAsync(CreateDetalleOrdenDto dto)
        {

            var validatorDto = await _createDetalleOrdenValidator.ValidateAsync(dto);

            if(!validatorDto.IsValid)
            {
                return OperationResultD<OrdenDetalleResponseDto>.Failure("Error al validar los datos de entrada: " + string.Join(", ", validatorDto.Errors.Select(e => e.ErrorMessage)));
            }


            // Validar que la orden exista y esté en estado pendiente antes de agregar el producto

            var orden = await _ordenRepository.GetByIdAsync(dto.OrdenId);

            if (orden == null) { 
            return OperationResultD<OrdenDetalleResponseDto>.Failure("Orden no encontrada");
            }

            if(orden.Estado != EstadoOrden.Pendiente)
            {
                return OperationResultD<OrdenDetalleResponseDto>.Failure("No se pueden agregar productos a una orden que no está pendiente");
            }




            // Validar que el producto exista y que la cantidad solicitada no exceda el stock disponible

            var producto = await _productoRepository.ObtenerConInventarioAsync(dto.ProductId);

            if (producto == null)
            {
                return OperationResultD<OrdenDetalleResponseDto>.Failure("Producto no encontrado");

            }

            // Validar que el producto tenga inventario asociado
            if (producto.Inventario == null)
            {
                return OperationResultD<OrdenDetalleResponseDto>.Failure("Inventario no encontrado para este producto");
            }


            // Validar que la cantidad solicitada no exceda el stock disponible
            if (dto.Cantidad > producto.Inventario.CantidadActual)
            {
                return OperationResultD<OrdenDetalleResponseDto>.Failure("Cantidad solicitada excede el stock disponible");
            }

            // Verificar si el producto ya existe en el detalle de la orden
            var detalleExistente = await _detalleOrdenRepository.ObtenerProductoEnOrdenAsync(dto.OrdenId, dto.ProductId);

            if (detalleExistente != null)
            {
                var nuevaCantidad = detalleExistente.Cantidad + dto.Cantidad;
                if(nuevaCantidad > producto.Inventario.CantidadActual)
                {
                    return OperationResultD<OrdenDetalleResponseDto>.Failure("La cantidad total del producto en la orden excede el stock disponible");
                }

                detalleExistente.ActualizarCantidad(nuevaCantidad);

                await _detalleOrdenRepository.UpdateAsync(detalleExistente);

                var resultadoRecalculoActualizado = await _ordenServices.RecalcularTotalAsync(dto.OrdenId);

                if(!resultadoRecalculoActualizado.IsSuccess)
                {   
                    return OperationResultD<OrdenDetalleResponseDto>.Failure("Error al recalcular el total de la orden después de actualizar la cantidad: " + resultadoRecalculoActualizado.Message);
                }


                var detalleOrdenResponseActualizado = _mapperDetalleOrdencs.MapToDetalleOrdenResponseDto(detalleExistente);

                return OperationResultD<OrdenDetalleResponseDto>.Success(detalleOrdenResponseActualizado, "Cantidad del producto actualizada en el detalle de orden exitosamente");

            }


            // Crear un nuevo detalle de orden utilizando el mapper


            var detalleOrden = _mapperDetalleOrdencs.MapToCreateDetalleOrden(dto, producto);


            // Guardar el nuevo detalle de orden en el repositorio

            await _detalleOrdenRepository.CreateAsync(detalleOrden);

          var detalleConProducto = await _detalleOrdenRepository.ObtenerConProductoAsync(detalleOrden.Id);

            if (detalleConProducto == null) 
            {
                return OperationResultD<OrdenDetalleResponseDto>.Failure("Error al obtener el detalle de orden con el producto");

            }

            // Recalcular el total de la orden después de agregar el nuevo detalle
            var resultadoRecalculo = await _ordenServices.RecalcularTotalAsync(dto.OrdenId);

            if(!resultadoRecalculo.IsSuccess)
            {
                return OperationResultD<OrdenDetalleResponseDto>.Failure("Error al recalcular el total de la orden: " + resultadoRecalculo.Message);
            }




            var detalleOrdenResponse = _mapperDetalleOrdencs.MapToDetalleOrdenResponseDto(detalleConProducto);

            return OperationResultD<OrdenDetalleResponseDto>.Success(detalleOrdenResponse, "Producto agregado al detalle de orden exitosamente");


        }








        // Implementación del método para eliminar un producto del detalle de la orden
        public async Task<OperationResultD<bool>> EliminarProductoAsync(int id)
        {

            var detalleOrden =  await _detalleOrdenRepository.GetByIdAsync(id);
            if (detalleOrden == null) 
            {
              return OperationResultD<bool>.Failure("Detalle de orden no encontrado");
            
            }




            var orden = await _ordenRepository.GetByIdAsync(detalleOrden.OrdenId);

            if(orden == null)
            {
                return OperationResultD<bool>.Failure("Orden no encontrada");
            }

            if(orden.Estado != EstadoOrden.Pendiente)
            {
                return OperationResultD<bool>.Failure("No se pueden eliminar productos de una orden que no está pendiente");
            }
          

            await _detalleOrdenRepository.DeleteAsync(detalleOrden.Id);

            

            var resultadoRecalculo = await _ordenServices.RecalcularTotalAsync(detalleOrden.OrdenId);
            if (!resultadoRecalculo.IsSuccess)
            {
                return OperationResultD<bool>.Failure("Error al recalcular el total de la orden: " + resultadoRecalculo.Message);
            }

            return OperationResultD<bool>.Success(true, "Producto eliminado del detalle de orden exitosamente");




        }

        // Implementación del método para obtener el detalle de una orden

        public async Task<OperationResultD<List<OrdenDetalleResponseDto>>> GetOrdenDetalle(int id)
        {
            var detalles = await _detalleOrdenRepository.ObtenerPorOrdenIdAsync(id);

            if(!detalles.Any())
            {
                return OperationResultD<List<OrdenDetalleResponseDto>>.Failure("No se encontraron detalles para esta orden");
            }

            var detallesResponse = detalles.Select(d => _mapperDetalleOrdencs.MapToDetalleOrdenResponseDto(d)).ToList();
            

            return OperationResultD<List<OrdenDetalleResponseDto>>.Success(detallesResponse, "Detalles de la orden obtenidos exitosamente");
        }
    }


    /*El sistema debe:
  • Registrar cada producto agregado a la orden.
  • Almacenar cantidad, precio unitario y subtotal.
 • Permitir modificar la cantidad antes de confirmar la orden.
  • Recalcular automáticamente el total si se modifica un producto
    */
}
