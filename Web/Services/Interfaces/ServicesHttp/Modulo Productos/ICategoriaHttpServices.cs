using Web.Models.Modelo_Productos.CategoriaModels;

namespace Web.Services.Interfaces.ServicesHttp.Modulo_Productos
{
    public interface ICategoriaHttpServices
    {
        public Task<List<CategoriaModel>> GetCategoriasAsync();

        public Task<CategoriaModel> GetCategoriaByIdAsync(int id);

        public Task<CategoriaModel> CreateCategoriaAsync(CreateCategoriaModel model);

        public Task<CategoriaModel> UpdateCategoriaAsync(UpdateCategoriaModel model);

        public Task<bool> DeleteCategoriaAsync(int id);

    }
}
