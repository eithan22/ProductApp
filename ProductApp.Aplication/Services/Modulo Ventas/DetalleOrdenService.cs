using ProductApp.Aplication.Dtos.Modulo_Ventas.DetalleOrdenDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Interface.IMappers.Modulo_Ventas;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Repository;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Services
{
    public class DetalleOrdenService : IDetalleOrdenServices
    {
        private readonly IDetalleOrdenRepository _detalleOrdenRepository;
        private readonly IOrdenServices _ordenServices;
        private readonly IProductoRepository _productoRepository;
        private readonly IMapperDetalleOrdencs _mapperDetalleOrdencs;

        public DetalleOrdenService(IDetalleOrdenRepository detalleOrdenRepository,
            IOrdenServices ordenServices,
            IProductoRepository productoRepository,
            IMapperDetalleOrdencs mapperDetalleOrdencs)
        {
            _detalleOrdenRepository = detalleOrdenRepository;
            _ordenServices = ordenServices;
            _productoRepository = productoRepository;
            _mapperDetalleOrdencs = mapperDetalleOrdencs;
        }

        // Implementación del método para actualizar la cantidad de un producto en el detalle de la orden
        public async Task<OperationResultD<OrdenDetalleResponseDto>> ActualizarDetalleOrden(int id, UpdateDetalleOrdenDto dto)
        {
            var detalleOrden = await _detalleOrdenRepository.GetByIdAsync(id);
            if (detalleOrden == null) 
            {
                return OperationResultD<OrdenDetalleResponseDto>.Failure("Detalle de orden no encontrado");

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
            var detalleConProducto = await _detalleOrdenRepository.ObtenerDetalleConProductoAsync(detalleOrden.Id);

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
            var producto = await _productoRepository.GetByIdAsync(dto.ProductId);

            if (producto == null)
            {
                return OperationResultD<OrdenDetalleResponseDto>.Failure("Producto no encontrado");
            }

            var detalleOrden = _mapperDetalleOrdencs.MapToCreateDetalleOrden(dto, producto);

            


            await _detalleOrdenRepository.CreateAsync(detalleOrden);

          var detalleConProducto = await _detalleOrdenRepository.ObtenerDetalleConProductoAsync(detalleOrden.Id);

            if (detalleConProducto == null) 
            {
                return OperationResultD<OrdenDetalleResponseDto>.Failure("Error al obtener el detalle de orden con el producto");

            }

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

            await _detalleOrdenRepository.DeleteAsync(detalleOrden.Id);

            

            var resultadoRecalculo = await _ordenServices.RecalcularTotalAsync(detalleOrden.OrdenId);
            if (!resultadoRecalculo.IsSuccess)
            {
                return OperationResultD<bool>.Failure("Error al recalcular el total de la orden: " + resultadoRecalculo.Message);
            }

            return OperationResultD<bool>.Success(true, "Producto eliminado del detalle de orden exitosamente");




        }

        // Implementación del método para obtener el detalle de una orden

        public async Task<OperationResultD<List<OrdenDetalleResponseDto>>> GetOrdenDetalle(int ordenId)
        {
            var detalles = await _detalleOrdenRepository.ObtenerDetalleOrdenPorOrdenIdAsync(ordenId);

            if(!detalles.Any())
            {
                return OperationResultD<List<OrdenDetalleResponseDto>>.Failure("No se encontraron detalles para esta orden");
            }

            var detallesResponse = detalles.Select(d => _mapperDetalleOrdencs.MapToDetalleOrdenResponseDto(d)).ToList();
            

            return OperationResultD<List<OrdenDetalleResponseDto>>.Success(detallesResponse, "Detalles de la orden obtenidos exitosamente");
        }
    }
}
