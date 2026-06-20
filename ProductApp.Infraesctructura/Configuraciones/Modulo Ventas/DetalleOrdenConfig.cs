using Microsoft.EntityFrameworkCore;
using ProductApp.Domian.Entitis;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Infraesctructura.Persistencia.Configuraciones
{
    public class DetalleOrdenConfig : IEntityTypeConfiguration<OrdenDetalle>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<OrdenDetalle> builder)
        {
                builder.HasOne(x => x.Producto)
                       .WithMany()
                       .HasForeignKey(x => x.ProductId);

                builder.HasOne(x => x.Orden)
                       .WithMany(x => x.Detalles)
                       .HasForeignKey(x => x.OrdenId);
           
        



        builder.Property(od => od.Cantidad)
                .IsRequired();


            builder.Property(od => od.PrecioUnitario)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(od => od.Subtotal)
                .HasColumnType("decimal(18,2)");
        }
    }
}
