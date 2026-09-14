using Microsoft.EntityFrameworkCore;
using ProductApp.Domian.Common.Enums.EnumsPago;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Contex;
using ProductApp.Infraesctructura.Persistencia.Repository.GeneryRepos;

namespace ProductApp.Infraesctructura.Persistencia.Repository
{
    public class PagoRepository : GenericRepository<Pago>, IPagoRepository
    {
        public PagoRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<Pago>> ObtenerPagosPorOrdenAsync(int ordenId)
        {
            return await _context.Pagos
                .Where(p => !p.EstaEliminado && p.OrdenId == ordenId)
                .ToListAsync();
        }

        public async Task<decimal> ObtenerTotalPagadoPorOrdenAsync(int ordenId)
        {
            return await _context.Pagos
                .Where(p => !p.EstaEliminado && p.OrdenId == ordenId)
                .SumAsync(p => p.Monto);
        }

        public async Task<(List<Pago> Items, int TotalCount, decimal TotalMonto, int OrdenesSaldadas,
              Dictionary<MetodoPago, decimal> MontoPorMetodo, HashSet<int> IdsPrimerPago)>
            ObtenerPagosPaginadosAsync(
                int? ordenId, DateTime? desde, DateTime? hasta,
                MetodoPago? metodoPago, int pageNumber, int pageSize)
        {
            var query = _context.Pagos
                .Include(p => p.Orden)
                    .ThenInclude(o => o.Cliente)
                .Where(p => !p.EstaEliminado)
                .AsQueryable();

            if (ordenId.HasValue)
            {
                query = query.Where(p => p.OrdenId == ordenId.Value);
            }

            if (desde.HasValue)
            {
                var fechaDesde = desde.Value.Date;
                query = query.Where(p => p.FechaPago >= fechaDesde);
            }

            if (hasta.HasValue)
            {
                // El "hasta" es inclusivo: se toma el día completo hasta las 23:59:59
                var fechaHasta = hasta.Value.Date.AddDays(1);
                query = query.Where(p => p.FechaPago < fechaHasta);
            }

            if (metodoPago.HasValue)
            {
                query = query.Where(p => p.MetodoPago == metodoPago.Value);
            }

            var totalCount = await query.CountAsync();
            var totalMonto = await query.SumAsync(p => (decimal?)p.Monto) ?? 0m;
            var ordenesSaldadas = await query.CountAsync(p => p.Estado == EstadoPago.Completado);

            var items = await query
                .OrderByDescending(p => p.FechaPago)
                .ThenByDescending(p => p.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var montoPorMetodo = await query
                .GroupBy(p => p.MetodoPago)
                .Select(g => new { Metodo = g.Key, Monto = g.Sum(p => p.Monto) })
                .ToDictionaryAsync(x => x.Metodo, x => x.Monto);

            var ordenIdsDePagina = items.Select(p => p.OrdenId).Distinct().ToList();
            var idsPrimerPago = (await _context.Pagos
                .Where(p => !p.EstaEliminado && ordenIdsDePagina.Contains(p.OrdenId))
                .GroupBy(p => p.OrdenId)
                .Select(g => g.OrderBy(p => p.FechaPago).ThenBy(p => p.Id).Select(p => p.Id).First())
                .ToListAsync())
                .ToHashSet();

            return (items, totalCount, totalMonto, ordenesSaldadas, montoPorMetodo, idsPrimerPago);
        }
    }
}
