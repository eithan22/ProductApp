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
        public DbSet<OrdenDetalle> DetalleOrden { get; set; }

        public DbSet<Cliente>Clientes { get; set; }

        public DbSet<Pago> Pagos { get; set; }

        public DbSet<Inventario> Inventario { get; set; }

        public DbSet<Categoria> Categorias { get; set; }

        public DbSet<ConfiguracionSistema> ConfiguracionSistema { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<ProductApp.Domian.Common.Base.BaseEntity>())
            {
                if (entry.State == EntityState.Modified)
                    entry.Entity.ActualizarFechaModificacion();
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}

