using ProductApp.Aplication.Dtos.PagoDto;
using ProductApp.Domian.Entitis;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Interface.IMappers.Modulo_Ventas
{
    public interface IMapperPago
    {
        Pago MapToCreatePago(CreatePagoDto dto);
        PagoResponseDto MapToPagoResponseDto(Pago pago, decimal saldoPendiente);
    }
}
