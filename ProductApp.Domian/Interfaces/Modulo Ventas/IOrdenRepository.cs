using ProductApp.Domian.Common.Enums.EnumsOrden;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces.IGeneryRepos;

namespace ProductApp.Domian.Interfaces
{
    public interface IOrdenRepository : IGenericRepository<Orden>
    {
        Task<List<Orden>> GetAllConDetallesAsync(EstadoOrden? estado = null);
        Task<List<Orden>> ObtenerPorClienteAsync(int clienteId, EstadoOrden? estado = null);
        Task<List<Orden>> ObtenerPorUsuarioAsync(int usuarioId);
        Task<List<Orden>> ObtenerPorRangoFechaAsync(DateTime desde, DateTime hasta, EstadoOrden? estado = null);
        Task<Orden?> GetByIdConClienteAsync(int id);
    }
}
