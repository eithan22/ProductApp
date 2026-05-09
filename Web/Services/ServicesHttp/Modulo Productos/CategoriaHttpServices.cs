using Web.Models.Modelo_Productos.CategoriaModels;
using Web.Services.Interfaces.IEndPoints.Modulo_Productos;
using Web.Services.Interfaces.ServicesHttp.Modulo_Productos;

namespace Web.Services.ServicesHttp.Modulo_Productos
{
    public class CategoriaHttpServices : ICategoriaHttpServices
    {
        private readonly HttpClient _httpClient;
        private readonly ICategoriaEndpoint _categoriaEndpoint;
        public CategoriaHttpServices(HttpClient httpClient, ICategoriaEndpoint categoriaEndpoint)
        {
            _httpClient = httpClient;
            _categoriaEndpoint = categoriaEndpoint;
        }
        public Task<CategoriaModel> CreateCategoriaAsync(CreateCategoriaModel model)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteCategoriaAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<CategoriaModel> GetCategoriaByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<CategoriaModel>> GetCategoriasAsync()
        {
            throw new NotImplementedException();
        }

        public Task<CategoriaModel> UpdateCategoriaAsync(UpdateCategoriaModel model)
        {
            throw new NotImplementedException();
        }
    }
}
