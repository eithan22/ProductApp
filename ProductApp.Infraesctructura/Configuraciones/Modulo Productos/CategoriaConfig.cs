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

            // Filtrado por EstaEliminado: ValidatorBusinessCategoria ya descarta las
            // categorías dadas de baja al comprobar el nombre (ExisteAsync filtra
            // !EstaEliminado). Sin el filtro, el nombre de una categoría eliminada
            // quedaría reservado para siempre.
            builder.HasIndex(c => c.Nombre)
                .IsUnique()
                .HasDatabaseName("UX_Categorias_Nombre")
                .HasFilter("[EstaEliminado] = 0");
        }
    }
}
