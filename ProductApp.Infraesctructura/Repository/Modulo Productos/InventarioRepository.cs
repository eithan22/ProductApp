using Microsoft.EntityFrameworkCore;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Contex;
using ProductApp.Infraesctructura.Persistencia.Repository.GeneryRepos;

namespace ProductApp.Infraesctructura.Persistencia.Repository
{
    public class InventarioRepository : GenericRepository<Inventario>, IInventarioRepository
    {
        public InventarioRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<Inventario>> GetAllConProductoAsync()
        {
            return await _context.Inventario
                .Include(i => i.Producto)
                .Where(i => !i.EstaEliminado)
                .ToListAsync();
        }

        public async Task<Inventario?> GetByProductoIdAsync(int productoId)
        {
            return await _context.Inventario
                .Include(i => i.Producto)
                .FirstOrDefaultAsync(i => !i.EstaEliminado && i.ProductoId == productoId);
        }

        // Método para obtener el inventario con stock bajo
        public async Task<List<Inventario>> GetStockBajoAsync()
        {
            return await _context.Inventario
                .Include(i => i.Producto)
                .Where(i => !i.EstaEliminado && i.CantidadActual <= i.CantidadMinima)
                .ToListAsync();
        }
    }
}
