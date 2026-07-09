using ProductApp.Aplication.Dtos.Modulo_Configuracion;
using Web.Models.Modelo_Configuracion;
using Web.Services.Interfaces.IBase;
using Web.Services.Interfaces.IEndPoints.Modulo_Configuracion;
using Web.Services.Interfaces.ServicesHttp.Modulo_Configuracion;

namespace Web.Services.ServicesHttp.Modulo_Configuracion
{
    public class ConfiguracionHttpServices : IConfiguracionHttpServices
    {
        private readonly IBaseHttpServices _baseHttpServices;
        private readonly IConfiguracionEndpoint _configuracionEndpoint;

        public ConfiguracionHttpServices(IBaseHttpServices baseHttpServices, IConfiguracionEndpoint configuracionEndpoint)
        {
            _baseHttpServices = baseHttpServices;
            _configuracionEndpoint = configuracionEndpoint;
        }

        public async Task<ConfiguracionModel> ObtenerAsync()
        {
            return await _baseHttpServices.GetAsync<ConfiguracionModel>(_configuracionEndpoint.Url);
        }

        public async Task<ConfiguracionModel> ActualizarAsync(ConfiguracionModel model)
        {
            var dto = new ActualizarConfiguracionSistemaDto
            {
                CantidadMinimaInventarioDefecto = model.CantidadMinimaInventarioDefecto,
                DuracionTokenMinutos = model.DuracionTokenMinutos,
                NombreEmpresa = model.NombreEmpresa,
                Moneda = model.Moneda
            };

            return await _baseHttpServices.PutAsync<ActualizarConfiguracionSistemaDto, ConfiguracionModel>(_configuracionEndpoint.Url, dto);
        }
    }
}
