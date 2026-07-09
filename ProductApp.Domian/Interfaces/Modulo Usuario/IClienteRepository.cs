using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces.IGeneryRepos;

namespace ProductApp.Domian.Interfaces
{
    public interface IClienteRepository : IGenericRepository<Cliente>
    {
        Task<(List<Cliente> Items, int TotalCount)> GetAllClientesAsync(bool incluirInactivos, int pageNumber, int pageSize);
        Task<List<Cliente>> BuscarClientesAsync(string? nombre, string? telefono, string? correo, bool incluirInactivos = false);
        Task<bool> ExistePorCorreoAsync(string correo);
        Task<bool> ExistePorCedulaAsync(string cedula);
    }
}
