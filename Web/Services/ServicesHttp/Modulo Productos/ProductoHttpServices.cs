using ProductApp.Aplication.Dtos.ProductoDto;
using Web.Models.Modelo_Productos.ProductoModels;
using Web.Services.Interfaces.IBase;
using Web.Services.Interfaces.IEndPoints.Modulo_Productos;
using Web.Services.Interfaces.ServicesHttp.Modulo_Productos;
using Web.Services.Mappers.Modulo_Productos;

namespace Web.Services.ServicesHttp.Modulo_Productos
{
    public class ProductoHttpServices : IProductoHttpServices
    {
        private readonly IBaseHttpServices _baseHttpServices;
        private readonly IProductoEndpoint _productoEndpoint;

        public ProductoHttpServices(IBaseHttpServices baseHttpServices, IProductoEndpoint productoEndpoint)
        {
            _baseHttpServices = baseHttpServices;
            _productoEndpoint = productoEndpoint;
        }

        public async Task<ProductoModel> CreateProductoAsync(CreateProductoModel model)
        {
            var dto = ProductoMapperM.MapAddProductoDto(model);
            return await _baseHttpServices.PostAsync<CreateProductoDto, ProductoModel>(_productoEndpoint.Create, dto);
        }

        public async Task<bool> DisableProductoAsync(int id)
        {
            await _baseHttpServices.PatchAsync<object, object>($"{_productoEndpoint.Disable}{id}", new { });
            return true;
        }

        public async Task<bool> EnableProductoAsync(int id)
        {
            await _baseHttpServices.PatchAsync<object, object>($"{_productoEndpoint.Enable}{id}", new { });
            return true;
        }

        public async Task<ProductoModel> GetProductoByIdAsync(int id)
        {
            return await _baseHttpServices.GetAsync<ProductoModel>($"{_productoEndpoint.GetById}{id}");
        }

        public async Task<List<ProductoModel>> GetProductosAsync()
        {
            return await _baseHttpServices.GetAsync<List<ProductoModel>>(_productoEndpoint.GetAll);
        }

        public async Task<List<ProductoModel>> BuscarProductosAsync(string? nombre, string? categoria)
        {
            return await _baseHttpServices.GetAsync<List<ProductoModel>>($"{_productoEndpoint.Buscar}?nombre={nombre}&categoria={categoria}");
        }

        public async Task<ProductoModel> UpdateProductoAsync(UpdateProductoModel model)
        {
            var dto = ProductoMapperM.MapUpdateProductoDto(model);
            return await _baseHttpServices.PutAsync<UpdateProductoDto, ProductoModel>($"{_productoEndpoint.Update}{model.Id}", dto);
        }
    }
}
