using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces.IGeneryRepos;

namespace ProductApp.Domian.Interfaces
{
    public interface IDetalleOrdenRepository : IGenericRepository<OrdenDetalle>
    {
        Task<List<OrdenDetalle>> ObtenerPorOrdenIdAsync(int ordenId);
        Task<OrdenDetalle?> ObtenerConProductoAsync(int id);
        Task<OrdenDetalle?> ObtenerProductoEnOrdenAsync(int ordenId, int productoId);
    }
}
