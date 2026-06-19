using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces.IGeneryRepos;

namespace ProductApp.Domian.Interfaces
{
    public interface IUsuarioRepository : IGenericRepository<Usuario>
    {
        Task<Usuario?> GetByEmailAsync(string email);
        Task<Usuario?> GetByUsernameAsync(string username);
    }
}
