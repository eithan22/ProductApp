using Microsoft.EntityFrameworkCore;
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

        public  Task<List<Producto>> BuscarProductosAsync(string? Nombre, string? Categoria)
        {
            var query = _context.Productos
            .Include(p => p.Categoria)
             .AsQueryable();

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
                .FirstOrDefaultAsync(p => p.Id == id);
        }




        public async Task<IEnumerable<Producto>> GetAllConCategoriaAsync()
        {
            return await _context.Productos
                .Include(p => p.Categoria)
                .ToListAsync();
        }

        public Task<Producto?> ObtenerConInventarioAsync(int id)
        {
            return _context.Productos
                .Include(p => p.Inventario)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}

            
            



        
    

