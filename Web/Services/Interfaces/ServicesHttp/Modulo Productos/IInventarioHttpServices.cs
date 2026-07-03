using Web.Models.Modelo_Productos.InventarioModels;

namespace Web.Services.Interfaces.ServicesHttp.Modulo_Productos
{
    public interface IInventarioHttpServices
    {
        Task<List<InventarioModel>> GetAllInventariosAsync();
        Task<List<InventarioModel>> GetStockBajoAsync();
        Task<InventarioModel> GetInventarioPorProductoAsync(int productoId);
        Task<InventarioModel> AgregarStockAsync(MovimientoStockModel model);
        Task<InventarioModel> DescontarStockAsync(MovimientoStockModel model);
        Task<InventarioModel> AjustarInventarioAsync(AjustarStockModel model);
    }
}
