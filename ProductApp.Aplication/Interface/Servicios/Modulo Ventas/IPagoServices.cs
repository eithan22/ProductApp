using ProductApp.Aplication.Dtos.PagoDto;
using ProductApp.Aplication.Result.OperationResult;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Interface
{
    public interface IPagoServices
    {
        Task<OperationResultD<PagoResponseDto>> RegistrarPagoAsync(CreatePagoDto dto, int usuarioSolicitanteId);
        Task<OperationResultD<List<PagoResponseDto>>> ObtenerPagosPorOrdenAsync(int ordenId);
        Task<OperationResultD<decimal>> ObtenerSaldoPendienteAsync(int ordenId);
    }
}
