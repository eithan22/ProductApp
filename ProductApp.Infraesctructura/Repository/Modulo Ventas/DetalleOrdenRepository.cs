using Microsoft.EntityFrameworkCore;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Contex;
using ProductApp.Infraesctructura.Persistencia.Repository.GeneryRepos;

namespace ProductApp.Infraesctructura.Persistencia.Repository
{
    public class DetalleOrdenRepository : GenericRepository<OrdenDetalle>, IDetalleOrdenRepository
    {
        public DetalleOrdenRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<OrdenDetalle?> ObtenerConProductoAsync(int id)
        {
            return await _context.DetalleOrden
                .Include(od => od.Producto)
                .FirstOrDefaultAsync(od => !od.EstaEliminado && od.Id == id);
        }

        public async Task<List<OrdenDetalle>> ObtenerPorOrdenIdAsync(int ordenId)
        {
            return await _context.DetalleOrden
                .Include(od => od.Producto)
                .Where(od => !od.EstaEliminado && od.OrdenId == ordenId)
                .ToListAsync();
        }

        public Task<OrdenDetalle?> ObtenerProductoEnOrdenAsync(int ordenId, int productoId)
        {
            return _context.DetalleOrden
                .Include(od => od.Producto)
                .FirstOrDefaultAsync(od => !od.EstaEliminado && od.ProductId == productoId && od.OrdenId == ordenId);
        }
    }
}
