using Microsoft.EntityFrameworkCore;
using ProductApp.Aplication.Helper;
using ProductApp.Domian.Common.Enums.EnumsUsuario;
using ProductApp.Domian.Entitis;
using ProductApp.Infraesctructura.Persistencia.Contex;

namespace ProductApp.Api.Seed
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(AppDbContext context, IConfiguration configuration)
        {
            if (!await context.Usuarios.AnyAsync())
            {
                var username = configuration["Seed:AdminUsername"];
                var email = configuration["Seed:AdminEmail"];
                var password = configuration["Seed:AdminPassword"];

                // Mismo criterio que Jwt:Key en Program.cs: si falta un valor obligatorio se
                // corta el arranque diciendo qué configurar y dónde. Sin esto el null viajaba
                // hasta el constructor de Usuario o hasta BCrypt, y la app moría con una
                // excepción que no menciona la configuración. La comprobación va acá y no en
                // Program.cs a propósito: estas tres claves solo se necesitan la primera vez,
                // cuando todavía no hay ningún usuario. Exigirlas en cada arranque rompería
                // los despliegues ya sembrados.
                if (string.IsNullOrWhiteSpace(username) ||
                    string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(password))
                    throw new InvalidOperationException(
                        "Falta configurar Seed:AdminUsername, Seed:AdminEmail y Seed:AdminPassword " +
                        "para crear el usuario administrador inicial. En desarrollo, usa " +
                        "dotnet user-secrets. En producción, configura las variables de entorno " +
                        "Seed__AdminUsername, Seed__AdminEmail y Seed__AdminPassword.");

                var admin = new Usuario("Administrador", email, username, RolUsuario.Administrador);
                admin.EstablecerPasswordHash(PasswordHelper.Hash(password));
                admin.MarcarPasswordComoTemporal();

                context.Usuarios.Add(admin);
                await context.SaveChangesAsync();
            }

            if (!await context.ConfiguracionSistema.AnyAsync())
            {
                var configuracionSistema = new ConfiguracionSistema(5, 60, "Mi Empresa", "USD");
                context.ConfiguracionSistema.Add(configuracionSistema);
                await context.SaveChangesAsync();
            }

            // Cliente reservado de las ventas de mostrador. A diferencia de los dos seeds
            // de arriba, la tabla de clientes sí tiene otros registros, así que la
            // condición no es "está vacía" sino "no existe todavía el reservado".
            if (!await context.Clientes.AnyAsync(c => c.Cedula == Cliente.CedulaConsumidorFinal))
            {
                var consumidorFinal = new Cliente(
                    Cliente.NombreConsumidorFinal,
                    Cliente.CedulaConsumidorFinal,
                    Cliente.DireccionConsumidorFinal,
                    Cliente.CorreoConsumidorFinal,
                    Cliente.TelefonoConsumidorFinal);

                context.Clientes.Add(consumidorFinal);
                await context.SaveChangesAsync();
            }
        }
    }
}
