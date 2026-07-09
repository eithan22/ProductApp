using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces.IGeneryRepos;

namespace ProductApp.Domian.Interfaces
{
    public interface ICategoriaRepository : IGenericRepository<Categoria>
    {
        Task<bool> ExistePorNombreAsync(string nombre);
        Task<Categoria?> GetByIdIncluyendoEliminadosAsync(int id);
        Task<(List<Categoria> Items, int TotalCount)> GetPagedAsync(bool incluirInactivos, int pageNumber, int pageSize);
    }
}
