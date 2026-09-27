using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces.IGeneryRepos;

namespace ProductApp.Domian.Interfaces
{
    public interface IProveedorRepository : IGenericRepository<Proveedor>
    {
        Task<(List<Proveedor> Items, int TotalCount)> GetAllProveedoresAsync(bool incluirInactivos, int pageNumber, int pageSize);
        Task<List<Proveedor>> BuscarProveedoresAsync(string? nombre, bool incluirInactivos = false);

        // Necesario para el borrado físico: con DeleteBehavior.Restrict la base rechazaría
        // la operación con un error de FK crudo, así que se consulta antes para poder
        // devolver un mensaje entendible desde la regla de negocio.
        Task<int> ContarProductosAsociadosAsync(int proveedorId);
    }
}
