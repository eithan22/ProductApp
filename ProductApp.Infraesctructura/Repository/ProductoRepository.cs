using Microsoft.EntityFrameworkCore;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Contex;

namespace ProductApp.Infraesctructura.Persistencia.Repository

{
    public class ProductoRepository : IProductoRepository

    {
        private readonly AppDbContext _Context;

        public ProductoRepository(AppDbContext context)
        {
            _Context = context;
        }


        public async Task<Producto> CreateAsync(Producto producto)
        {
            await _Context.Productos.AddAsync(producto);
            await _Context.SaveChangesAsync();
            return producto;

        }

        public async Task DeleteAsync(int id)
        {
            var producto = _Context.Productos.Find(id);
            if(producto != null)
            {
                _Context.Productos.Remove(producto);
                await _Context.SaveChangesAsync();
            }

        }

        public async Task DisebleAsync(int id)
        {
            var producto = await _Context.Productos.FindAsync(id);
            if (producto != null)
            {
                producto.IsDisable = true;
                _Context.Productos.Update(producto);
                await _Context.SaveChangesAsync();
            }


        }

        public async Task<IEnumerable<Producto>> GetAllAsync()
        {
            var productos = await _Context.Productos.ToListAsync();


            return productos;

        }

        public async Task<Producto?> GetByIdAsync(int id)
        {
            var productos = await _Context.Productos.FindAsync(id);

            return productos;

        }

        public Task UpdateAsync(Producto producto)
        {
          var productos =  _Context.Productos.Update(producto);
            return _Context.SaveChangesAsync();







        }
    }
}

            
            



        
    

