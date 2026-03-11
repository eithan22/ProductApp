using Microsoft.EntityFrameworkCore;
using ProductApp.Domian.Entitis;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Infraesctructura.Persistencia.Configuraciones
{
    public class DetalleOrdenConfig : IEntityTypeConfiguration<OrderDetalle>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<OrderDetalle> builder)
        {
            builder.Property(od => od.Cantidad)
                .IsRequired();


            builder.Property(od => od.PrecioUnitario)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

           

        }
    }
}
