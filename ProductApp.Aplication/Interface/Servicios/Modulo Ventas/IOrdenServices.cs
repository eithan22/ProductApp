using ProductApp.Aplication.Dtos.Modulo_Ventas.OrdenDto;
using ProductApp.Aplication.Dtos.OrdenDto;
using ProductApp.Aplication.Interface.Servicios.BaseServices;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Common.Enums.EnumsOrden;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Interface
{
    public interface IOrdenServices
    {
          Task<OperationResultD<OrdenResponseDto>> CrearOrden(CreateOrdenDto dto, int usuarioId);
          Task<OperationResultD<bool>> CancelarOrden(int id, int usuarioSolicitanteId);
          Task<OperationResultD<bool>> ConfirmarOrden(int id, int usuarioSolicitanteId);
            Task<OperationResultD<List<OrdenResponseDto>>> ConsultarOrdenesPorFecha(DateTime fecha, EstadoOrden? estado = null);

        Task<OperationResultD<List<OrdenResponseDto>>> ConsultarOrdenesPorCliente(int clienteId, EstadoOrden? estado = null);


        Task<OperationResultD<List<OrdenResponseDto>>> GetAllOrdenes(EstadoOrden? estado = null);

        Task<OperationResultD<bool>> CambiarEstadoOrden(CambiarEstadoOrdenDto dto);

        Task<OperationResultD<bool>> RecalcularTotalAsync(int id);
        Task<OperationResultD<OrdenResponseDto>> GetOrdenByIdAsync(int id);
        Task<OperationResultD<List<OrdenResponseDto>>> GetOrdenesByUsuarioAsync(int usuarioId);










    }
}
