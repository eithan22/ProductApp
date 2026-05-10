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
       




        
    }
}

            
            



        
    

