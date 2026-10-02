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

        public async Task<(List<Inventario> Items, int TotalCount)> GetAllConProductoAsync(int pageNumber, int pageSize, int? proveedorId = null)
        {
            var query = _context.Inventario
                .Include(i => i.Producto)
                    .ThenInclude(p => p.Proveedor)
                .Where(i => !i.EstaEliminado);

            if (proveedorId.HasValue)
            {
                query = query.Where(i => i.Producto.ProveedorId == proveedorId.Value);
            }

            var totalCount = await query.CountAsync();

            // Sin OrderBy, SQL Server no garantiza el orden entre páginas: un registro de
            // inventario podría repetirse o desaparecer al paginar. Inventario no tiene nombre
            // propio, así que se ordena por el nombre del producto que ya viene incluido.
            var items = await query
                .OrderBy(i => i.Producto.Nombre)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Inventario?> GetByProductoIdAsync(int productoId)
        {
            return await _context.Inventario
                .Include(i => i.Producto)
                    .ThenInclude(p => p.Proveedor)
                .FirstOrDefaultAsync(i => !i.EstaEliminado && i.ProductoId == productoId);
        }

        // Método para obtener el inventario con stock bajo
        public async Task<List<Inventario>> GetStockBajoAsync(int? proveedorId = null)
        {
            var query = _context.Inventario
                .Include(i => i.Producto)
                    .ThenInclude(p => p.Proveedor)
                .Where(i => !i.EstaEliminado && i.CantidadActual <= i.CantidadMinima);

            // Filtro por proveedor (RF-3.7.3): permite ver de una sola vez todo lo que hay
            // que reponerle a un mismo proveedor.
            if (proveedorId.HasValue)
            {
                query = query.Where(i => i.Producto.ProveedorId == proveedorId.Value);
            }

            return await query.ToListAsync();
        }
    }
}
