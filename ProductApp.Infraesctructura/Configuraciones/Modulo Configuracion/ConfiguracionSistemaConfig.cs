using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductApp.Domian.Entitis;

namespace ProductApp.Infraesctructura.Persistencia.Configuraciones
{
    public class ConfiguracionSistemaConfig : IEntityTypeConfiguration<ConfiguracionSistema>
    {
        public void Configure(EntityTypeBuilder<ConfiguracionSistema> builder)
        {
            builder.Property(c => c.NombreEmpresa)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Moneda)
                .IsRequired()
                .HasMaxLength(10);
        }
    }
}
