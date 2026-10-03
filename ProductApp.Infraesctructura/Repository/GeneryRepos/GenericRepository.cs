using Microsoft.EntityFrameworkCore;
using ProductApp.Domian.Common.Base;
using ProductApp.Domian.Interfaces.IGeneryRepos;
using ProductApp.Infraesctructura.Persistencia.Contex;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ProductApp.Infraesctructura.Persistencia.Repository.GeneryRepos
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
    {

        // Los repositorios específicos (ProductoRepository, OrdenRepository, etc.) heredan de
        // GenericRepository<T> y reciben este mismo _context por constructor: así todas las
        // operaciones de una misma petición comparten el mismo DbContext y el mismo SaveChangesAsync.
        protected readonly AppDbContext _context;
        protected  DbSet<T> Entity => _context.Set<T>();


        public GenericRepository(AppDbContext context  )
        {
            _context = context;
        }


        public async Task<T> CreateAsync(T entity)
        {
            await Entity.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
         
             

        }


        // delete logico no se va a usar

        
        public async Task DisableAsync(int id)
        {
            var entity = await Entity.FindAsync(id);
            if (entity != null)
            {
                entity.Eliminar();
                await _context.SaveChangesAsync();
            }
        }
        


        public async Task DeleteAsync(int id)
        {
            var entity = await Entity.FindAsync(id);

            if (entity != null)
            {
                Entity.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        // Lectura completa sin paginar (ej. combos de categorías). Excluye siempre los soft-deleted.
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await Entity
                .AsNoTracking()
                .Where(x => !x.EstaEliminado)
                .ToListAsync();
        }

        // Versión paginada de GetAllAsync: devuelve la página pedida junto con el total de
        // registros, que la capa de arriba usa para calcular TotalPages.
        public async Task<(List<T> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
        {
            var query = Entity.AsNoTracking().Where(x => !x.EstaEliminado);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<T?> GetByIdAsync(int id)
        {
           var result = await Entity
               .Where(x => !x.EstaEliminado && x.Id == id)
               .FirstOrDefaultAsync();



            return result;

        }

        public async Task UpdateAsync(T entity)
        {
            Entity.Update(entity);
            await _context.SaveChangesAsync();

        }

        public async Task<bool> ExisteAsync(Expression<Func<T, bool>> filtro)
        {
            return await _context.Set<T>()
                .Where(x => !x.EstaEliminado)
                .AnyAsync(filtro);
        }

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> filtro)
        {
           return await Entity.Where(x => !x.EstaEliminado).FirstOrDefaultAsync(filtro);
        }
    }
}
