using Microsoft.EntityFrameworkCore;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Contex;
using ProductApp.Infraesctructura.Persistencia.Repository.GeneryRepos;

namespace ProductApp.Infraesctructura.Persistencia.Repository
{
    public class NotificacionRepository : GenericRepository<Notificacion>, INotificacionRepository
    {
        public NotificacionRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<Notificacion>> ObtenerRecientesPorUsuarioAsync(int usuarioId, int cantidad)
        {
            return await _context.Notificaciones
                .Where(n => !n.EstaEliminado && n.UsuarioId == usuarioId)
                .OrderByDescending(n => n.CreadoEn)
                .Take(cantidad)
                .ToListAsync();
        }

        public async Task<int> ContarNoLeidasPorUsuarioAsync(int usuarioId)
        {
            return await _context.Notificaciones
                .CountAsync(n => !n.EstaEliminado && n.UsuarioId == usuarioId && !n.Leida);
        }

        public async Task MarcarTodasComoLeidasAsync(int usuarioId)
        {
            var pendientes = await _context.Notificaciones
                .Where(n => !n.EstaEliminado && n.UsuarioId == usuarioId && !n.Leida)
                .ToListAsync();

            foreach (var notificacion in pendientes)
                notificacion.MarcarComoLeida();

            await _context.SaveChangesAsync();
        }
    }
}
