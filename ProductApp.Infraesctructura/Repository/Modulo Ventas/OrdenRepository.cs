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

        public async Task<List<(Orden Orden, int CantidadProductos, decimal TotalPagado)>> BuscarOrdenesAsync(string texto)
        {
            var query = _context.Ordenes
                .Include(o => o.Cliente)
                .Where(o => !o.EstaEliminado && o.Estado != EstadoOrden.Cancelada)
                .AsQueryable();

            // El "número de orden" que ve el usuario es el Id: no hay un campo aparte.
            // Se acepta con o sin '#' porque es como aparece escrito en toda la interfaz.
            if (int.TryParse(texto.TrimStart('#'), out var numeroOrden))
                query = query.Where(o => o.Id == numeroOrden || o.Cliente.Nombre.Contains(texto));
            else
                query = query.Where(o => o.Cliente.Nombre.Contains(texto));

            // La cantidad de productos y el total pagado se calculan en la misma consulta:
            // traerlos por separado significaría una consulta extra por cada orden hallada.
            var resultados = await query
                .OrderByDescending(o => o.Fecha)
                .ThenByDescending(o => o.Id)
                .Select(o => new
                {
                    Orden = o,
                    CantidadProductos = o.Detalles.Count(d => !d.EstaEliminado),
                    TotalPagado = o.Pagos.Where(p => !p.EstaEliminado).Sum(p => (decimal?)p.Monto) ?? 0m
                })
                .ToListAsync();

            return resultados
                .Select(r => (r.Orden, r.CantidadProductos, r.TotalPagado))
                .ToList();
        }
    }
}
