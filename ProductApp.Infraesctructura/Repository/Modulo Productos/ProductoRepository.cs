using Microsoft.EntityFrameworkCore;
using ProductApp.Domian.Common.Enums.EnumsProducto;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Contex;
using ProductApp.Infraesctructura.Persistencia.Repository.GeneryRepos;

namespace ProductApp.Infraesctructura.Persistencia.Repository

{
    public class ProductoRepository : GenericRepository<Producto>, IProductoRepository

    {
        public ProductoRepository (AppDbContext context) : base(context)
        {

        }

        public  Task<List<Producto>> BuscarProductosAsync(string? Nombre, string? Categoria, bool incluirInactivos = false)
        {
            var query = _context.Productos
            .Include(p => p.Categoria)
            .Where(p => !p.EstaEliminado)
            .AsQueryable();

            if (!incluirInactivos)
            {
                query = query.Where(p => p.Estado == EstadoProducto.Activo);
            }

            if (!string.IsNullOrWhiteSpace(Nombre))
            {
                query = query.Where(p => p.Nombre.Contains(Nombre));
            }

            if (!string.IsNullOrWhiteSpace(Categoria))
            {
                query = query.Where(p => p.Categoria.Nombre.Contains(Categoria));
            }
            return query.ToListAsync();



        }

        //sin usar aun es para traer la categoria cuando usameos el getByIdProductos
        public async Task<Producto?> GetProductoConCategoriaByIdAsync(int id)
        {
            return await _context.Productos
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => !p.EstaEliminado && p.Id == id);
        }




        public async Task<(List<Producto> Items, int TotalCount)> GetAllConCategoriaAsync(bool incluirInactivos, int pageNumber, int pageSize)
        {
            var query = _context.Productos
                .Include(p => p.Categoria)
                .Where(p => !p.EstaEliminado)
                .AsQueryable();

            if (!incluirInactivos)
            {
                query = query.Where(p => p.Estado == EstadoProducto.Activo);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public Task<Producto?> ObtenerConInventarioAsync(int id)
        {
            return _context.Productos
                .Include(p => p.Inventario)
                .FirstOrDefaultAsync(p => !p.EstaEliminado && p.Id == id);
        }
    }
}

            
            



        
    

