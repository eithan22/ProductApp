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
            if (await context.Usuarios.AnyAsync())
                return;

            var username = configuration["Seed:AdminUsername"]!;
            var email = configuration["Seed:AdminEmail"]!;
            var password = configuration["Seed:AdminPassword"]!;

            var admin = new Usuario("Administrador", email, username, RolUsuario.Administrador);
            admin.EstablecerPasswordHash(PasswordHelper.Hash(password));
            admin.MarcarPasswordComoTemporal();

            context.Usuarios.Add(admin);
            await context.SaveChangesAsync();
        }
    }
}
