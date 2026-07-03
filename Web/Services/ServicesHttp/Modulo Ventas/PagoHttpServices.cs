using ProductApp.Aplication.Dtos.PagoDto;
using Web.Models.Modelo_Ventas.PagoModels;
using Web.Services.Interfaces.IBase;
using Web.Services.Interfaces.IEndPoints.Modulo_Ventas;
using Web.Services.Interfaces.ServicesHttp.Modulo_Ventas;
using Web.Services.Mappers.Modulo_Ventas;

namespace Web.Services.ServicesHttp.Modulo_Ventas
{
    public class PagoHttpServices : IPagoHttpServices
    {
        private readonly IBaseHttpServices _baseHttpServices;
        private readonly IPagoEndpoint _pagoEndpoint;

        public PagoHttpServices(IBaseHttpServices baseHttpServices, IPagoEndpoint pagoEndpoint)
        {
            _baseHttpServices = baseHttpServices;
            _pagoEndpoint = pagoEndpoint;
        }

        public async Task<PagoModel> RegistrarPagoAsync(CreatePagoModel model)
        {
            var dto = PagoMapperM.MapCreatePagoDto(model);
            return await _baseHttpServices.PostAsync<CreatePagoDto, PagoModel>(_pagoEndpoint.RegistrarPago, dto);
        }

        public async Task<List<PagoModel>> GetPagosPorOrdenAsync(int ordenId)
        {
            return await _baseHttpServices.GetAsync<List<PagoModel>>($"{_pagoEndpoint.GetPagosByOrden}{ordenId}");
        }

        public async Task<decimal> GetSaldoPendienteAsync(int ordenId)
        {
            return await _baseHttpServices.GetAsync<decimal>($"{_pagoEndpoint.GetSaldoPendiente}{ordenId}");
        }
    }
}
