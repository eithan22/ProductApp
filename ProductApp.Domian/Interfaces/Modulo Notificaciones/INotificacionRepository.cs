using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces.IGeneryRepos;

namespace ProductApp.Domian.Interfaces
{
    public interface INotificacionRepository : IGenericRepository<Notificacion>
    {
        Task<List<Notificacion>> ObtenerRecientesPorUsuarioAsync(int usuarioId, int cantidad);
        Task<int> ContarNoLeidasPorUsuarioAsync(int usuarioId);
        Task MarcarTodasComoLeidasAsync(int usuarioId);
    }
}
