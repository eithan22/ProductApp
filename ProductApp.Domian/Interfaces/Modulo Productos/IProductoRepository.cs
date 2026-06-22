using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces.IGeneryRepos;

namespace ProductApp.Domian.Interfaces
{
    public interface IProductoRepository : IGenericRepository<Producto>
    {
        Task<IEnumerable<Producto>> GetAllConCategoriaAsync();
        Task<List<Producto>> BuscarProductosAsync(string? nombre, string? categoria);
        Task<Producto?> ObtenerConInventarioAsync(int id);
        Task<Producto?> GetProductoConCategoriaByIdAsync(int id);
    }
}
