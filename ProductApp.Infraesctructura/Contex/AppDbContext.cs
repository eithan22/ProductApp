using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProductApp.Domian.Common.Exceptions;
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

        public DbSet<Notificacion> Notificaciones { get; set; }

        public DbSet<Proveedor> Proveedores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        // Índice único → mensaje de negocio. Si se renombra un índice en un *Config.cs,
        // hay que renombrarlo acá también (viven en la misma carpeta a propósito).
        private static readonly Dictionary<string, string> MensajesPorIndice = new()
        {
            ["UX_Usuarios_Email"] = "Ya existe un usuario con ese correo electrónico.",
            ["UX_Usuarios_Username"] = "Ya existe un usuario con ese nombre de usuario.",
            ["UX_Clientes_Cedula"] = "Ya existe un cliente con esa cédula.",
            ["UX_Clientes_Correo"] = "Ya existe un cliente con ese correo electrónico.",
            ["UX_Categorias_Nombre"] = "Ya existe una categoría con ese nombre.",
            ["UX_Productos_Nombre_CategoriaId"] = "Ya existe un producto con ese nombre en esa categoría.",
            ["UX_Proveedores_Nombre"] = "Ya existe un proveedor con ese nombre.",
            ["UX_Proveedores_Correo"] = "Ya existe un proveedor con ese correo electrónico.",
        };

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<ProductApp.Domian.Common.Base.BaseEntity>())
            {
                if (entry.State == EntityState.Modified)
                    entry.Entity.ActualizarFechaModificacion();
            }

            try
            {
                return await base.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqlException sql
                                                && (sql.Number == 2601 || sql.Number == 2627))
            {
                var indice = MensajesPorIndice.Keys.FirstOrDefault(k => sql.Message.Contains(k));
                var mensaje = indice is not null
                    ? MensajesPorIndice[indice]
                    : "Ya existe un registro con esos datos.";
                throw new DuplicadoException(mensaje);
            }
        }
    }
}

