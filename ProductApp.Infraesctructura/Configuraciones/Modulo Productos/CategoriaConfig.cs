using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductApp.Domian.Entitis;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Infraesctructura.Persistencia.Configuraciones
{
    public class CategoriaConfig : IEntityTypeConfiguration<Categoria>

    {
        public void Configure(EntityTypeBuilder<Categoria> builder)
        {
           builder.Property(c => c.Nombre)
                 .IsRequired()
                 .HasMaxLength(50);

             builder.Property(c => c.Descripcion)
                 .HasMaxLength(100);
        }
    }
}
