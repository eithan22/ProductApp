using ProductApp.Aplication.Common;
using Web.Models.Modelo_Productos.InventarioModels;

namespace Web.Services.Interfaces.ServicesHttp.Modulo_Productos
{
    public interface IInventarioHttpServices
    {
        // proveedorId null = sin filtrar por proveedor.
        Task<PagedResult<InventarioModel>> GetAllInventariosAsync(int pageNumber = 1, int pageSize = PaginacionDefaults.PageSizeDefault, int? proveedorId = null);
        Task<List<InventarioModel>> GetStockBajoAsync(int? proveedorId = null);
        Task<InventarioModel> GetInventarioPorProductoAsync(int productoId);
        Task<InventarioModel> AgregarStockAsync(MovimientoStockModel model);
        Task<InventarioModel> DescontarStockAsync(MovimientoStockModel model);
        Task<InventarioModel> AjustarInventarioAsync(AjustarStockModel model);
    }
}
