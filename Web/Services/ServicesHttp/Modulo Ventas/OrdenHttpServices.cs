using ProductApp.Aplication.Dtos.Modulo_Ventas.OrdenDto;
using ProductApp.Aplication.Dtos.OrdenDto;
using Web.Models.Modelo_Ventas.OrdenModels;
using Web.Services.Interfaces.IBase;
using Web.Services.Interfaces.IEndPoints.Modulo_Ventas;
using Web.Services.Interfaces.ServicesHttp.Modulo_Ventas;
using Web.Services.Mappers.Modulo_Ventas;

namespace Web.Services.ServicesHttp.Modulo_Ventas
{
    public class OrdenHttpServices : IOrdenHttpServices
    {
        private readonly IBaseHttpServices _baseHttpServices;
        private readonly IOrdenEndpoint _ordenEndpoint;

        public OrdenHttpServices(IBaseHttpServices baseHttpServices, IOrdenEndpoint ordenEndpoint)
        {
            _baseHttpServices = baseHttpServices;
            _ordenEndpoint = ordenEndpoint;
        }

        public async Task<OrdenModel> CreateOrdenAsync(CreateOrdenModel model)
        {
            var dto = OrdenMapperM.MapCreateOrdenDto(model);
            return await _baseHttpServices.PostAsync<CreateOrdenDto, OrdenModel>(_ordenEndpoint.Create, dto);
        }

        public async Task<List<OrdenModel>> GetOrdenesAsync(string? estado = null)
        {
            var query = string.IsNullOrWhiteSpace(estado) ? "" : $"?estado={estado}";
            return await _baseHttpServices.GetAsync<List<OrdenModel>>($"{_ordenEndpoint.GetAll}{query}");
        }

        public async Task<OrdenModel> GetOrdenByIdAsync(int id)
        {
            return await _baseHttpServices.GetAsync<OrdenModel>($"{_ordenEndpoint.GetById}{id}");
        }

        public async Task<List<OrdenModel>> GetOrdenesByClienteAsync(int clienteId, string? estado = null)
        {
            var query = string.IsNullOrWhiteSpace(estado) ? "" : $"?estado={estado}";
            return await _baseHttpServices.GetAsync<List<OrdenModel>>($"{_ordenEndpoint.GetByCliente}{clienteId}{query}");
        }

        public async Task<List<OrdenModel>> GetOrdenesByUsuarioAsync(int usuarioId)
        {
            return await _baseHttpServices.GetAsync<List<OrdenModel>>($"{_ordenEndpoint.GetByUsuario}{usuarioId}");
        }

        public async Task<List<OrdenModel>> GetOrdenesByFechaAsync(DateTime fecha, string? estado = null)
        {
            var query = string.IsNullOrWhiteSpace(estado) ? "" : $"?estado={estado}";
            return await _baseHttpServices.GetAsync<List<OrdenModel>>($"{_ordenEndpoint.GetByFecha}{fecha:yyyy-MM-dd}{query}");
        }

        public async Task<bool> CambiarEstadoAsync(CambiarEstadoOrdenModel model)
        {
            var dto = OrdenMapperM.MapCambiarEstadoOrdenDto(model);
            await _baseHttpServices.PatchAsync<CambiarEstadoOrdenDto, object>(_ordenEndpoint.CambiarEstado, dto);
            return true;
        }

        public async Task<bool> CancelarOrdenAsync(int id)
        {
            await _baseHttpServices.PatchAsync<object, object>($"{_ordenEndpoint.Cancelar}{id}", new { });
            return true;
        }

        public async Task<bool> ConfirmarOrdenAsync(int id)
        {
            await _baseHttpServices.PatchAsync<object, object>($"{_ordenEndpoint.Confirmar}{id}", new { });
            return true;
        }
    }
}
