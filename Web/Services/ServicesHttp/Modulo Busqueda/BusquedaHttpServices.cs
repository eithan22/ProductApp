using Web.Models.Modelo_Busqueda.BusquedaModels;
using Web.Services.Interfaces.IBase;
using Web.Services.Interfaces.IEndPoints.Modulo_Busqueda;
using Web.Services.Interfaces.ServicesHttp.Modulo_Busqueda;

namespace Web.Services.ServicesHttp.Modulo_Busqueda
{
    public class BusquedaHttpServices : IBusquedaHttpServices
    {
        private readonly IBaseHttpServices _baseHttpServices;
        private readonly IBusquedaEndpoint _busquedaEndpoint;

        public BusquedaHttpServices(IBaseHttpServices baseHttpServices, IBusquedaEndpoint busquedaEndpoint)
        {
            _baseHttpServices = baseHttpServices;
            _busquedaEndpoint = busquedaEndpoint;
        }

        public async Task<BusquedaGlobalModel> BuscarGlobalAsync(string texto)
        {
            // El texto lo escribe el usuario libremente: sin escapar, un "&" o un "#"
            // cortaría la query string y la API recibiría el término a medias.
            return await _baseHttpServices.GetAsync<BusquedaGlobalModel>(
                $"{_busquedaEndpoint.GetBuscarGlobal}?texto={Uri.EscapeDataString(texto)}");
        }
    }
}
