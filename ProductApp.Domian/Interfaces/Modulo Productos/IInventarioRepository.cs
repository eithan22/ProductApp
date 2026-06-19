using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces.IGeneryRepos;

namespace ProductApp.Domian.Interfaces
{
    public interface IInventarioRepository : IGenericRepository<Inventario>
    {
        Task<Inventario?> GetByProductoIdAsync(int productoId);
        Task<List<Inventario>> GetStockBajoAsync();
        Task<List<Inventario>> GetAllConProductoAsync();
    }
}
