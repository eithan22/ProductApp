using Microsoft.EntityFrameworkCore;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Contex;
using ProductApp.Infraesctructura.Persistencia.Repository.GeneryRepos;

namespace ProductApp.Infraesctructura.Persistencia.Repository
{
    public class CategoriaRepository : GenericRepository<Categoria>, ICategoriaRepository
    {
        public CategoriaRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<bool> ExistePorNombreAsync(string nombre)
        {
            return await _context.Categorias
                .AnyAsync(c => !c.EstaEliminado && c.Nombre == nombre);
        }

        public Task<Categoria?> GetByIdIncluyendoEliminadosAsync(int id)
        {
            return _context.Categorias.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<(List<Categoria> Items, int TotalCount)> GetPagedAsync(bool incluirInactivos, int pageNumber, int pageSize)
        {
            var query = _context.Categorias.AsQueryable();

            if (!incluirInactivos)
            {
                query = query.Where(c => !c.EstaEliminado);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
