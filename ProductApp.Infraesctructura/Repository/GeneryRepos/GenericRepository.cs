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
    public class GenericRepository<T> : IGeneryRepository<T> where T : BaseEntity
    {

        //Aun no comprendi la inyeccion de el context para los demas repositorios
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

        /*
        public async Task DisebleAsync(int id)
        {
            var entity = await Entity.FindAsync(id);
            if (entity != null)
            {
                entity.IsDisable = true;
                await _context.SaveChangesAsync();


            }
        }
        */


        public async Task DeleteAsync(int id)
        {
            var entity = await Entity.FindAsync(id);

            if (entity != null)
            {
                Entity.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        //aun no lo entiendo . 
        public async Task<IEnumerable<T>> GetAllAsync()
        {
         var result =  await Entity.
                Where(x => !x.IsDisable).
                ToListAsync();

            return result;

        }

        //aun no lo entiendo.

        public async Task<T?> GetByIdAsync(int id)
        {
           var result = await Entity.
               Where(x => !x.IsDisable && x.Id == id).
               FirstOrDefaultAsync();



            return result;

        }

        public async Task UpdateAsync(T entity)
        {
            Entity.Update(entity);
            await _context.SaveChangesAsync();

        }

        public async Task<bool> ExisteAsync(Expression<Func<T, bool>> filtro)
        {
            return await _context.Set<T>().AnyAsync(filtro);
        }

    }
}
