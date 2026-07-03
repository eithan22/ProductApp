using ProductApp.Aplication.Dtos.Modulo_Ventas.DetalleOrdenDto;
using Web.Models.Modelo_Ventas.DetalleOrdenModels;
using Web.Services.Interfaces.IBase;
using Web.Services.Interfaces.IEndPoints.Modulo_Ventas;
using Web.Services.Interfaces.ServicesHttp.Modulo_Ventas;
using Web.Services.Mappers.Modulo_Ventas;

namespace Web.Services.ServicesHttp.Modulo_Ventas
{
    public class DetalleOrdenHttpServices : IDetalleOrdenHttpServices
    {
        private readonly IBaseHttpServices _baseHttpServices;
        private readonly IDetalleOrdenEndpoint _detalleOrdenEndpoint;

        public DetalleOrdenHttpServices(IBaseHttpServices baseHttpServices, IDetalleOrdenEndpoint detalleOrdenEndpoint)
        {
            _baseHttpServices = baseHttpServices;
            _detalleOrdenEndpoint = detalleOrdenEndpoint;
        }

        public async Task<DetalleOrdenModel> AgregarProductoAsync(CreateDetalleOrdenModel model)
        {
            var dto = DetalleOrdenMapperM.MapCreateDetalleOrdenDto(model);
            return await _baseHttpServices.PostAsync<CreateDetalleOrdenDto, DetalleOrdenModel>(_detalleOrdenEndpoint.Create, dto);
        }

        public async Task<List<DetalleOrdenModel>> GetDetallesPorOrdenAsync(int ordenId)
        {
            return await _baseHttpServices.GetAsync<List<DetalleOrdenModel>>($"{_detalleOrdenEndpoint.GetByOrden}{ordenId}");
        }

        public async Task<DetalleOrdenModel> ActualizarCantidadAsync(UpdateDetalleOrdenModel model)
        {
            var dto = DetalleOrdenMapperM.MapUpdateDetalleOrdenDto(model);
            return await _baseHttpServices.PatchAsync<UpdateDetalleOrdenDto, DetalleOrdenModel>($"{_detalleOrdenEndpoint.Update}{model.Id}", dto);
        }

        public async Task<bool> EliminarProductoAsync(int id)
        {
            await _baseHttpServices.DeleteAsync($"{_detalleOrdenEndpoint.Delete}{id}");
            return true;
        }
    }
}
