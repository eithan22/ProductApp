using ProductApp.Aplication.Dtos.PagoDto;
using ProductApp.Aplication.Interface.IMappers.Modulo_Ventas;
using ProductApp.Domian.Common.Enums.EnumsPago;
using ProductApp.Domian.Entitis;

namespace ProductApp.Aplication.Mappers.Modulo_Ventas
{
    public class PagoMapper : IMapperPago
    {
        public Pago MapToCreatePago(CreatePagoDto dto)
        {
            var metodoPago = Enum.Parse<MetodoPago>(dto.MetodoPago, true);
            return new Pago(dto.OrdenId, dto.Monto, metodoPago);
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
