using ProductApp.Aplication.Dtos.CategoriaDto;
using Web.Models.Modelo_Productos.CategoriaModels;
using Web.Services.Interfaces.IBase;
using Web.Services.Interfaces.IEndPoints.Modulo_Productos;
using Web.Services.Interfaces.ServicesHttp.Modulo_Productos;
using Web.Services.Mappers.Modulo_Productos;

namespace Web.Services.ServicesHttp.Modulo_Productos
{
    public class CategoriaHttpServices : ICategoriaHttpServices
    {
        private readonly IBaseHttpServices _baseHttpServices;
        private readonly ICategoriaEndpoint _categoriaEndpoint;

        public CategoriaHttpServices(IBaseHttpServices baseHttpServices, ICategoriaEndpoint categoriaEndpoint)
        {
            _baseHttpServices = baseHttpServices;
            _categoriaEndpoint = categoriaEndpoint;
        }

        public async Task<CategoriaModel> CreateCategoriaAsync(CreateCategoriaModel model)
        {
            var dto = CategoriaMapperM.MapAddCategoriaDto(model);
            return await _baseHttpServices.PostAsync<CreateCategoriaDto, CategoriaModel>(_categoriaEndpoint.Create, dto);
        }

        public async Task<bool> DeleteCategoriaAsync(int id)
        {
            await _baseHttpServices.PatchAsync<object, object>($"{_categoriaEndpoint.Disable}{id}", new { });
            return true;
        }

        public async Task<CategoriaModel> GetCategoriaByIdAsync(int id)
        {
            return await _baseHttpServices.GetAsync<CategoriaModel>($"{_categoriaEndpoint.GetById}{id}");
        }

        public async Task<List<CategoriaModel>> GetCategoriasAsync()
        {
            return await _baseHttpServices.GetAsync<List<CategoriaModel>>(_categoriaEndpoint.GetAll);
        }

        public async Task<CategoriaModel> UpdateCategoriaAsync(UpdateCategoriaModel model)
        {
            var dto = CategoriaMapperM.MapUpdateCategoriaDto(model);
            return await _baseHttpServices.PutAsync<UpdateCategoriaDto, CategoriaModel>($"{_categoriaEndpoint.Update}{model.Id}", dto);
        }
    }
}
