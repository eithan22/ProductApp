using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductApp.Domian.Entitis;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace ProductApp.Infraesctructura.Persistencia.Configuraciones
{
    public class ProductoConfig : IEntityTypeConfiguration<Producto>
    {
        public void Configure(EntityTypeBuilder<Producto> builder)
        {
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

                builder.Property(e => e.Estado)
                    .HasConversion<string>()
                .IsRequired();





            }
        }
    }
}
