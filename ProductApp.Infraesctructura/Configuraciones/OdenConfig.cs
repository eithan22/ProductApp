using Microsoft.EntityFrameworkCore;
using ProductApp.Domian.Entitis;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Infraesctructura.Persistencia.Configuraciones
{
    public class OdenConfig : IEntityTypeConfiguration<Orden>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Orden> builder)
        {
            builder.Property(o => o.Fecha)
                .IsRequired()
                .HasColumnType("Date");

            builder.Property(o => o.Total)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(o => o.Estado)
             .IsRequired()
             .HasConversion<string>();


                



        }
    }
}
