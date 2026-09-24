using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductApp.Domian.Entitis;

namespace ProductApp.Infraesctructura.Persistencia.Configuraciones
{
    public class ProductoConfig : IEntityTypeConfiguration<Producto>
    {
        public void Configure(EntityTypeBuilder<Producto> builder)
        {
            builder.Property(e => e.Nombre)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(e => e.Descripcion)
                .HasMaxLength(100);

            builder.Property(e => e.Precio)
                .HasColumnType("decimal(18,2)");

            builder.Property(e => e.Costo)
                .HasColumnType("decimal(18,2)");

            builder.Property(e => e.ImagenUrl)
                .HasMaxLength(500);

            builder.Property(e => e.Estado)
                .HasConversion<string>()
                .IsRequired();

            // Restrict y no Cascade: borrar un proveedor no puede llevarse por delante su
            // catálogo de productos. El intento se corta antes, en la regla de negocio del
            // borrado físico, con un mensaje entendible.
            builder.HasOne(e => e.Proveedor)
                .WithMany(p => p.Productos)
                .HasForeignKey(e => e.ProveedorId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Único por categoría, no global: así lo trata la importación masiva
            // (ImportacionProductosService) y así queda alineado el validator (ver
            // ValidatorBusinessProducto). "Camiseta" puede existir en Ropa y en Promociones.
            builder.HasIndex(e => new { e.Nombre, e.CategoriaId })
                .IsUnique()
                .HasDatabaseName("UX_Productos_Nombre_CategoriaId")
                .HasFilter("[EstaEliminado] = 0");
        }
    }
}
