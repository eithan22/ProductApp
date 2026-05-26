using ProductApp.Aplication.Dtos.Modulo_Ventas.OrdenDto;
using ProductApp.Aplication.Dtos.OrdenDto;
using ProductApp.Aplication.Interface.Servicios.BaseServices;
using ProductApp.Aplication.Result.OperationResult;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Interface
{
    public interface IOrdenServices 
    {
          Task<OperationResultD<OrdenResponseDto>> CrearOrden(CreateOrdenDto dto);
          Task<OperationResultD<bool>> CancelarOrden(int id);
            Task<OperationResultD<List<OrdenResponseDto>>> ConsultarOrdenesPorFecha(DateTime fecha);

        Task<OperationResultD<List<OrdenResponseDto>>> ConsultarOrdenesPorCliente(int clienteId);


        Task<OperationResultD<List<OrdenResponseDto>>> GetAllOrdenes();

        Task<OperationResultD<bool>> CambiarEstadoOrden(CambiarEstadoOrdenDto dto);

        Task<OperationResultD<bool>> RecalcularTotalAsync(int id);










    }
}
