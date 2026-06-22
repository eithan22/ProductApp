using Microsoft.EntityFrameworkCore;
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
    }
}
