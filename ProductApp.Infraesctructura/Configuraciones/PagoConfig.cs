using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductApp.Domian.Entitis;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Infraesctructura.Persistencia.Configuraciones
{
    public class PagoConfig : IEntityTypeConfiguration<Pago>
    {
        public void Configure(EntityTypeBuilder<Pago> builder)
        {
            builder.Property(p => p.Monto)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(p => p.FechaPago)
                .IsRequired();


            builder.Property(p => p.Estado)
                .HasConversion<string>()
                .IsRequired();

            builder.Property(p => p.MetodoPago)
                .HasConversion<string>()
                .IsRequired();

               

        }
    }
}
