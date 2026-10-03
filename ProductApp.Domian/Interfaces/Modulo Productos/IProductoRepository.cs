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

        // Necesario para el borrado físico: con DeleteBehavior.Restrict la base rechazaría
        // la operación con un error de FK crudo, así que se consulta antes para poder
        // devolver un mensaje entendible desde la regla de negocio.
        Task<int> ContarOrdenDetallesAsociadosAsync(int productoId);
    }
}
