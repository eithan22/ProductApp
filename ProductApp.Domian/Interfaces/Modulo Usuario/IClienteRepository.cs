using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces.IGeneryRepos;

namespace ProductApp.Domian.Interfaces
{
    public interface IClienteRepository : IGenericRepository<Cliente>
    {
        Task<List<Cliente>> BuscarClientesAsync(string? nombre, string? telefono, string? correo);
        Task<bool> ExistePorCorreoAsync(string correo);
        Task<bool> ExistePorCedulaAsync(string cedula);
    }
}
