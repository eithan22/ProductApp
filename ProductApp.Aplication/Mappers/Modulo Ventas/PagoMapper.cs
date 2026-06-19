using ProductApp.Aplication.Dtos.PagoDto;
using ProductApp.Aplication.Interface.IMappers.Modulo_Ventas;
using ProductApp.Domian.Entitis;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Mappers.Modulo_Ventas
{
    public class PagoMapper : IMapperPago
    {
        public Pago MapToCreatePago(CreatePagoDto dto)
        {
            return new Pago(dto.OrdenId, dto.Monto, dto.MetodoPago);
        }

        public PagoResponseDto MapToPagoResponseDto(Pago pago, decimal saldoPendiente)
        {
            return new PagoResponseDto
            {
                Id             = pago.Id,
                Monto          = pago.Monto,
                MetodoPago     = pago.MetodoPago.ToString(),
                EstadoPago     = pago.Estado.ToString(),
                FechaPago      = pago.FechaPago,
                SaldoPendiente = saldoPendiente
            };
        }
    }
}
