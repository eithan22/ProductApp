using ProductApp.Aplication.Common;
using ProductApp.Aplication.Dtos.Modulo_Proveedores.ProveedorDto;
using Web.Models.Modelo_Proveedores.ProveedorModels;
using Web.Services.Interfaces.IBase;
using Web.Services.Interfaces.IEndPoints.Modulo_Proveedores;
using Web.Services.Interfaces.ServicesHttp.Modulo_Proveedores;
using Web.Services.Mappers.Modulo_Proveedores;

namespace Web.Services.ServicesHttp.Modulo_Proveedores
{
    public class ProveedorHttpServices : IProveedorHttpServices
    {
        private readonly IBaseHttpServices _baseHttpServices;
        private readonly IProveedorEndpoint _proveedorEndpoint;

        public ProveedorHttpServices(IBaseHttpServices baseHttpServices, IProveedorEndpoint proveedorEndpoint)
        {
            _baseHttpServices = baseHttpServices;
            _proveedorEndpoint = proveedorEndpoint;
        }

        public async Task<PagedResult<ProveedorModel>> GetProveedoresAsync(bool incluirInactivos = false, int pageNumber = 1, int pageSize = 10)
        {
            return await _baseHttpServices.GetAsync<PagedResult<ProveedorModel>>(
                $"{_proveedorEndpoint.GetAll}?incluirInactivos={incluirInactivos}&pageNumber={pageNumber}&pageSize={pageSize}");
        }

        public async Task<List<ProveedorModel>> GetProveedoresActivosAsync()
        {
            // Mismo criterio que las categorías del selector de producto: una sola
            // página grande en vez de paginar un desplegable.
            var paged = await _baseHttpServices.GetAsync<PagedResult<ProveedorModel>>(
                $"{_proveedorEndpoint.GetAll}?incluirInactivos=false&pageSize=100");
            return paged.Items;
        }

        public async Task<ProveedorModel> GetProveedorByIdAsync(int id)
        {
            return await _baseHttpServices.GetAsync<ProveedorModel>($"{_proveedorEndpoint.GetById}{id}");
        }

        public async Task<ProveedorModel> CreateProveedorAsync(CreateProveedorModel model)
        {
            var dto = ProveedorMapperM.MapAddProveedorDto(model);
            return await _baseHttpServices.PostAsync<CreateProveedorDto, ProveedorModel>(_proveedorEndpoint.Create, dto);
        }

        public async Task<ProveedorModel> UpdateProveedorAsync(UpdateProveedorModel model)
        {
            var dto = ProveedorMapperM.MapUpdateProveedorDto(model);
            return await _baseHttpServices.PutAsync<UpdateProveedorDto, ProveedorModel>($"{_proveedorEndpoint.Update}{model.Id}", dto);
        }

        public async Task<List<ProveedorModel>> BuscarProveedoresAsync(string? nombre, bool incluirInactivos = false)
        {
            return await _baseHttpServices.GetAsync<List<ProveedorModel>>(
                $"{_proveedorEndpoint.GetBuscar}?nombre={nombre}&incluirInactivos={incluirInactivos}");
        }

        public async Task<bool> DisableProveedorAsync(int id)
        {
            await _baseHttpServices.PatchAsync<object, object>($"{_proveedorEndpoint.Disable}{id}", new { });
            return true;
        }

        public async Task<bool> EnableProveedorAsync(int id)
        {
            await _baseHttpServices.PatchAsync<object, object>($"{_proveedorEndpoint.Enable}{id}", new { });
            return true;
        }
    }
}
