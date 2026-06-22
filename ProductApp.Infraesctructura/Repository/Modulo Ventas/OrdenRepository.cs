using Microsoft.EntityFrameworkCore;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Contex;
using ProductApp.Infraesctructura.Persistencia.Repository.GeneryRepos;

namespace ProductApp.Infraesctructura.Persistencia.Repository
{
    public class OrdenRepository : GenericRepository<Orden>, IOrdenRepository
    {
        public OrdenRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<Orden>> GetAllConDetallesAsync()
        {
            return await _context.Ordenes
                .Include(o => o.Cliente)
                .Include(o => o.Detalles)
                    .ThenInclude(d => d.Producto)
                .Where(o => !o.EstaEliminado)
                .ToListAsync();
        }

        public async Task<List<Orden>> ObtenerPorClienteAsync(int clienteId)
        {
            return await _context.Ordenes
                .Include(o => o.Cliente)
                .Include(o => o.Detalles)
                .Where(o => !o.EstaEliminado && o.ClienteId == clienteId)
                .ToListAsync();
        }

        public async Task<List<Orden>> ObtenerPorUsuarioAsync(int usuarioId)
        {
            return await _context.Ordenes
                .Include(o => o.Cliente)
                .Include(o => o.Detalles)
                .Where(o => !o.EstaEliminado && o.UsuarioId == usuarioId)
                .ToListAsync();
        }

        public async Task<List<Orden>> ObtenerPorRangoFechaAsync(DateTime desde, DateTime hasta)
        {
            return await _context.Ordenes
                .Include(o => o.Cliente)
                .Where(o => !o.EstaEliminado && o.Fecha >= desde && o.Fecha <= hasta)
                .ToListAsync();
        }

        public async Task<Orden?> GetByIdConClienteAsync(int id)
        {
            return await _context.Ordenes
                .Include(o => o.Cliente)
                .FirstOrDefaultAsync(o => o.Id == id && !o.EstaEliminado);
        }
    }
}
