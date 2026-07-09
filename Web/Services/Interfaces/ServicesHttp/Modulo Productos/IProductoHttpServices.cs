using ProductApp.Aplication.Common;
using Web.Models.Modelo_Productos.ProductoModels;

namespace Web.Services.Interfaces.ServicesHttp.Modulo_Productos
{
    public interface IProductoHttpServices
    {
        Task<PagedResult<ProductoModel>> GetProductosAsync(bool incluirInactivos = false, int pageNumber = 1, int pageSize = 10);
        Task<ProductoModel> GetProductoByIdAsync(int id);
        Task<ProductoModel> CreateProductoAsync(CreateProductoModel model);
        Task<ProductoModel> UpdateProductoAsync(UpdateProductoModel model);
        Task<bool> DisableProductoAsync(int id);
        Task<bool> EnableProductoAsync(int id);
        Task<List<ProductoModel>> BuscarProductosAsync(string? nombre, string? categoria);
    }
}
