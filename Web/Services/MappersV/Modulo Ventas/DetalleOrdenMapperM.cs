using ProductApp.Aplication.Dtos.Modulo_Ventas.DetalleOrdenDto;
using Web.Models.Modelo_Ventas.DetalleOrdenModels;

namespace Web.Services.Mappers.Modulo_Ventas
{
    public class DetalleOrdenMapperM
    {
        public static CreateDetalleOrdenDto MapCreateDetalleOrdenDto(CreateDetalleOrdenModel model)
        {
            return new CreateDetalleOrdenDto
            {
                ProductId = model.ProductId,
                Cantidad = model.Cantidad,
                OrdenId = model.OrdenId
            };
        }

        public static UpdateDetalleOrdenDto MapUpdateDetalleOrdenDto(UpdateDetalleOrdenModel model)
        {
            return new UpdateDetalleOrdenDto
            {
                id = model.Id,
                Cantidad = model.Cantidad
            };
        }
    }
}
