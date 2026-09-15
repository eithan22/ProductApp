using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductApp.Domian.Entitis;

namespace ProductApp.Infraesctructura.Persistencia.Configuraciones
{
    public class ProveedorConfig : IEntityTypeConfiguration<Proveedor>
    {
        public void Configure(EntityTypeBuilder<Proveedor> builder)
        {
            builder.Property(p => p.Nombre)
                .IsRequired()
                .HasMaxLength(60);

            builder.Property(p => p.Correo)
                .IsRequired()
                .HasMaxLength(60);

            builder.Property(p => p.Telefono)
                .IsRequired()
                .HasMaxLength(12);

            builder.Property(p => p.Direccion)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(p => p.Estado)
                .IsRequired()
                .HasConversion<string>();
        }
    }
}
