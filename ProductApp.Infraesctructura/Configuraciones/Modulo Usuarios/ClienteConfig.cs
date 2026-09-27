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
                .HasMaxLength(100);

            builder.Property(c => c.Correo)
                .IsRequired()
                .HasMaxLength(30);

             builder.Property(c => c.Telefono)
                .HasMaxLength(12);

            builder.Property(c => c.Cedula)
                .IsRequired()
                .HasMaxLength(13);
            
             builder.Property(c => c.Direccion)
                .HasMaxLength(100);

            builder.Property(c => c.Estado)
                .IsRequired()
            .HasConversion<string>();

            // Filtrado por la misma razón que en Usuario (ver UsuarioConfig): ExisteAsync
            // filtra !EstaEliminado, así que el índice tiene que filtrar igual.
            // Ojo con lo que NO está acá: Telefono y Nombre también los valida
            // ValidatorBusinessClientes como únicos, pero quedaron deliberadamente fuera del
            // índice — un teléfono compartido entre familiares y dos clientes homónimos son
            // casos reales, y esa regla queda como hallazgo aparte, no como restricción de base.
            builder.HasIndex(c => c.Cedula)
                .IsUnique()
                .HasDatabaseName("UX_Clientes_Cedula")
                .HasFilter("[EstaEliminado] = 0");

            builder.HasIndex(c => c.Correo)
                .IsUnique()
                .HasDatabaseName("UX_Clientes_Correo")
                .HasFilter("[EstaEliminado] = 0");
        }
    }
}
