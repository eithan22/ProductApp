using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces.IGeneryRepos;

namespace ProductApp.Domian.Interfaces
{
    public interface IProductoRepository : IGenericRepository<Producto>
    {
        Task<(List<Producto> Items, int TotalCount)> GetAllConCategoriaAsync(bool incluirInactivos, int pageNumber, int pageSize);
        Task<List<Producto>> BuscarProductosAsync(string? nombre, string? categoria, bool incluirInactivos = false);
        Task<Producto?> ObtenerConInventarioAsync(int id);
        Task<Producto?> GetProductoConCategoriaByIdAsync(int id);
    }
}
