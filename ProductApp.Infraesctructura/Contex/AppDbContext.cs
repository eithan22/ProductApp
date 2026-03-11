using Microsoft.EntityFrameworkCore;
using ProductApp.Domian.Entitis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ProductApp.Infraesctructura.Persistencia.Contex
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Producto> Productos { get; set; }
        public DbSet<Orden> Ordenes { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<OrderDetalle> DetalleOrden { get; set; }

        public DbSet<Cliente>Clientes { get; set; }

        public DbSet<Pago> Pagos { get; set; }

        public DbSet<Inventario> Inventarios { get; set; }

        public DbSet<Categoria> Categorias { get; set; }    

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());//agregar las configuraciones 

        }

    }
}

