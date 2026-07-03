using ProductApp.Aplication.Dtos.PagoDto;
using Web.Models.Modelo_Ventas.PagoModels;

namespace Web.Services.Mappers.Modulo_Ventas
{
    public class PagoMapperM
    {
        public static CreatePagoDto MapCreatePagoDto(CreatePagoModel model)
        {
            return new CreatePagoDto
            {
                OrdenId = model.OrdenId,
                Monto = model.Monto,
                MetodoPago = model.MetodoPago
            };
        }
    }
}
