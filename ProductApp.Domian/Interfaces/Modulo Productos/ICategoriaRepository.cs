using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces.IGeneryRepos;

namespace ProductApp.Domian.Interfaces
{
    public interface ICategoriaRepository : IGenericRepository<Categoria>
    {
        Task<bool> ExistePorNombreAsync(string nombre);
    }
}
