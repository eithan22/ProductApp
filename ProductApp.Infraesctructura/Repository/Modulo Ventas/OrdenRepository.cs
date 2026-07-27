using Microsoft.EntityFrameworkCore;
using ProductApp.Domian.Common.Enums.EnumsOrden;
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

        public async Task<List<Orden>> GetAllConDetallesAsync(EstadoOrden? estado = null)
        {
            var query = _context.Ordenes
                .Include(o => o.Cliente)
                .Include(o => o.Detalles)
                    .ThenInclude(d => d.Producto)
                .Where(o => !o.EstaEliminado)
                .AsQueryable();

            query = estado.HasValue
                ? query.Where(o => o.Estado == estado.Value)
                : query.Where(o => o.Estado != EstadoOrden.Cancelada);

            return await query.ToListAsync();
        }

        public async Task<List<Orden>> ObtenerPorClienteAsync(int clienteId, EstadoOrden? estado = null)
        {
            var query = _context.Ordenes
                .Include(o => o.Cliente)
                .Include(o => o.Detalles)
                .Where(o => !o.EstaEliminado && o.ClienteId == clienteId)
                .AsQueryable();

            query = estado.HasValue
                ? query.Where(o => o.Estado == estado.Value)
                : query.Where(o => o.Estado != EstadoOrden.Cancelada);

            return await query.ToListAsync();
        }

        public async Task<List<Orden>> ObtenerPorUsuarioAsync(int usuarioId)
        {
            return await _context.Ordenes
                .Include(o => o.Cliente)
                .Include(o => o.Detalles)
                .Where(o => !o.EstaEliminado && o.UsuarioId == usuarioId)
                .ToListAsync();
        }

        public async Task<List<Orden>> ObtenerPorRangoFechaAsync(DateTime desde, DateTime hasta, EstadoOrden? estado = null)
        {
            var query = _context.Ordenes
                .Include(o => o.Cliente)
                .Where(o => !o.EstaEliminado && o.Fecha >= desde && o.Fecha <= hasta)
                .AsQueryable();

            query = estado.HasValue
                ? query.Where(o => o.Estado == estado.Value)
                : query.Where(o => o.Estado != EstadoOrden.Cancelada);

            return await query.ToListAsync();
        }

        public async Task<Orden?> GetByIdConClienteAsync(int id)
        {
            return await _context.Ordenes
                .Include(o => o.Cliente)
                .FirstOrDefaultAsync(o => o.Id == id && !o.EstaEliminado);
        }
    }
}
