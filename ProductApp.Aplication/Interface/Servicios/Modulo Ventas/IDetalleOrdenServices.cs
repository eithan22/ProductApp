using ProductApp.Aplication.Dtos.Modulo_Ventas.DetalleOrdenDto;
using ProductApp.Aplication.Interface.Servicios.BaseServices;
using ProductApp.Aplication.Result.OperationResult;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Interface
{
    public interface IDetalleOrdenServices 
    {
        //agregar producto a la orden
        Task<OperationResultD<OrdenDetalleResponseDto>> AgregarProductoAsync(CreateDetalleOrdenDto dto, int usuarioSolicitanteId, bool esAdministrador);

        //modificar cantidad del producto
        Task<OperationResultD<OrdenDetalleResponseDto>> ActualizarDetalleOrden(int id, UpdateDetalleOrdenDto dto, int usuarioSolicitanteId, bool esAdministrador);

        //eliminar producto de la orden
        Task<OperationResultD<bool>> EliminarProductoAsync(int id, int usuarioSolicitanteId, bool esAdministrador);

        //consultar detalle de la orden
        Task<OperationResultD<List<OrdenDetalleResponseDto>>> GetOrdenDetalle(int id);



    }
}
