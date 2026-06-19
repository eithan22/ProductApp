using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces.IGeneryRepos;

namespace ProductApp.Domian.Interfaces
{
    public interface IOrdenRepository : IGenericRepository<Orden>
    {
        Task<List<Orden>> GetAllConDetallesAsync();
        Task<List<Orden>> ObtenerPorClienteAsync(int clienteId);
        Task<List<Orden>> ObtenerPorUsuarioAsync(int usuarioId);
        Task<List<Orden>> ObtenerPorRangoFechaAsync(DateTime desde, DateTime hasta);
    }
}
