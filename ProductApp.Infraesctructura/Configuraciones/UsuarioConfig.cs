using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductApp.Domian.Entitis;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Infraesctructura.Persistencia.Configuraciones
{
    public class UsuarioConfig : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {

            builder.Property(u => u.Nombre)
                .IsRequired()
                .HasMaxLength(20);
            

            builder.Property(u => u.Password)
                .IsRequired()
                .HasMaxLength(20);



            builder.Property(u => u.Edad)
                .IsRequired();

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.RolUsuario)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(u => u.EstadoUsuario)
                .IsRequired()
                .HasConversion<string>();











        }
    }
}
