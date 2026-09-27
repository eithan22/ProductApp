using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces.IGeneryRepos;

namespace ProductApp.Domian.Interfaces
{
    public interface IInventarioRepository : IGenericRepository<Inventario>
    {
        Task<Inventario?> GetByProductoIdAsync(int productoId);

        // proveedorId opcional (RF-3.7.3): si viene, solo devuelve el inventario de los
        // productos de ese proveedor. Va al final y con default para no romper las
        // llamadas que ya existen.
        Task<List<Inventario>> GetStockBajoAsync(int? proveedorId = null);
        Task<(List<Inventario> Items, int TotalCount)> GetAllConProductoAsync(int pageNumber, int pageSize, int? proveedorId = null);
    }
}
