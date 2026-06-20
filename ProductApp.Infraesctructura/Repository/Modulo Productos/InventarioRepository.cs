using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using ProductApp.Domian.Interfaces.IGeneryRepos;
using ProductApp.Infraesctructura.Persistencia.Contex;
using ProductApp.Infraesctructura.Persistencia.Repository.GeneryRepos;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

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
                .ToListAsync();
        }

        public async Task<Inventario?> GetByProductoIdAsync(int productoId)
        {

            return await _context.Inventario
                .Include(i => i.Producto)
                .FirstOrDefaultAsync(i => i.ProductoId == productoId);

        }

        // Método para obtener el inventario con stock bajo
        public async Task<List<Inventario>> GetStockBajoAsync()
        {
            return await _context.Inventario
                .Include(i => i.Producto)
                .Where(i => i.CantidadActual < i.CantidadMinima)
                .ToListAsync();

        }
    }
}
