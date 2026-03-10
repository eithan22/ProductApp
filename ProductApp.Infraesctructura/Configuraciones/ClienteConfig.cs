using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductApp.Domian.Entitis;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Infraesctructura.Persistencia.Configuraciones
{
    public class ClienteConfig : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> builder)
        {
           builder.Property(c => c.Nombre)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(c => c.Email)
                .IsRequired()
                .HasMaxLength(30);

             builder.Property(c => c.Telefono)
                .HasMaxLength(20);

            builder.Property(c => c.Cedula)
                .IsRequired()
                .HasMaxLength(13);
            
             builder.Property(c => c.Direccion)
                .HasMaxLength(50);

            builder.Property(c => c.Estado)
                .IsRequired()
            .HasConversion<string>();

        }
    }
}
