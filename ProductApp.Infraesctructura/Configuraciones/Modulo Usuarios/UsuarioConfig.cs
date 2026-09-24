using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductApp.Domian.Entitis;

namespace ProductApp.Infraesctructura.Persistencia.Configuraciones
{
    public class UsuarioConfig : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.Property(u => u.Nombre)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(u => u.Username)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.FechaNacimiento);

            builder.Ignore(u => u.Edad);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(u => u.RolUsuario)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(u => u.EstadoUsuario)
                .IsRequired()
                .HasConversion<string>();

            // Índice único FILTRADO, no total: la baja de un usuario es lógica (EstaEliminado),
            // así que la fila dada de baja sigue en la tabla y un índice sin filtro dejaría ese
            // email y ese username reservados para siempre. Con el filtro la unicidad queda
            // exactamente donde ya la valida ValidatorBusinessUsuarios (ExisteAsync filtra
            // !EstaEliminado). Índice y validador dicen lo mismo.
            //
            // El nombre se fija a mano (UX_) en vez de dejarlo en la convención (IX_) porque
            // GlobalExceptionHandler lo usa como clave para traducir el error 2601 de SQL
            // Server al mensaje de negocio correcto, y el prefijo UX_ es lo que permite al
            // detector reconocerlo dentro del mensaje sin depender del idioma del motor.
            // Renombrar un índice acá obliga a renombrarlo allá.
            builder.HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("UX_Usuarios_Email")
                .HasFilter("[EstaEliminado] = 0");

            builder.HasIndex(u => u.Username)
                .IsUnique()
                .HasDatabaseName("UX_Usuarios_Username")
                .HasFilter("[EstaEliminado] = 0");
        }
    }
}
