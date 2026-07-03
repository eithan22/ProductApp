using ProductApp.Aplication.Dtos.Modulo_Ventas.OrdenDto;
using ProductApp.Aplication.Dtos.OrdenDto;
using Web.Models.Modelo_Ventas.OrdenModels;

namespace Web.Services.Mappers.Modulo_Ventas
{
    public class OrdenMapperM
    {
        public static CreateOrdenDto MapCreateOrdenDto(CreateOrdenModel model)
        {
            return new CreateOrdenDto
            {
                ClienteId = model.ClienteId
            };
        }

        public static CambiarEstadoOrdenDto MapCambiarEstadoOrdenDto(CambiarEstadoOrdenModel model)
        {
            return new CambiarEstadoOrdenDto
            {
                Id = model.Id,
                NuevoEstado = model.NuevoEstado
            };
        }
    }
}
