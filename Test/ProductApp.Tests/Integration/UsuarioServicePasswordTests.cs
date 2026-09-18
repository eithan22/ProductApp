using FluentAssertions;
using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto;
using ProductApp.Aplication.Helper;
using ProductApp.Domian.Common.Enums.EnumsUsuario;
using ProductApp.Domian.Entitis;
using Xunit;

namespace ProductApp.Tests.Integration
{
    // Cubre el ciclo de cambio de contraseña obligatorio (DebeCambiarPassword) que
    // RequiereCambioPasswordFilter usa para bloquear la API: crear/resetear deja la bandera
    // en true (MarcarPasswordComoTemporal), y el cambio voluntario la apaga (ConfirmarCambioPassword).
    public class UsuarioServicePasswordTests
    {
        private static async Task<Usuario> SembrarUsuarioConPasswordAsync(
            Infraesctructura.Persistencia.Contex.AppDbContext context, string passwordActual = "Actual@123")
        {
            var usuario = new Usuario("Usuario Test", "usuario@test.com", "usuario.test", RolUsuario.Vendedor);
            usuario.EstablecerPasswordHash(PasswordHelper.Hash(passwordActual));
            usuario.MarcarPasswordComoTemporal();
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();
            return usuario;
        }

        [Fact]
        public async Task CambiarPasswordUsuario_ConContraseñaActualIncorrecta_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var usuario = await SembrarUsuarioConPasswordAsync(context);
            var service = IntegrationTestFactory.CrearUsuarioService(context);

            var resultado = await service.CambiarPasswordUsuario(new ChangePasswordDto
            {
                Id = usuario.Id,
                PasswordActual = "Incorrecta@1",
                PasswordNueva = "Nueva@12345"
            });

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("Contraseña actual incorrecta");
        }

        [Fact]
        public async Task CambiarPasswordUsuario_ConDatosValidos_ActualizaHashYQuitaLaBanderaDeCambioPendiente()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var usuario = await SembrarUsuarioConPasswordAsync(context, "Actual@123");
            var service = IntegrationTestFactory.CrearUsuarioService(context);

            var resultado = await service.CambiarPasswordUsuario(new ChangePasswordDto
            {
                Id = usuario.Id,
                PasswordActual = "Actual@123",
                PasswordNueva = "Nueva@12345"
            });

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            usuario.DebeCambiarPassword.Should().BeFalse();
            PasswordHelper.Verify("Nueva@12345", usuario.PasswordHash).Should().BeTrue();
        }

        [Fact]
        public async Task CambiarPasswordUsuario_ConUsuarioInexistente_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearUsuarioService(context);

            var resultado = await service.CambiarPasswordUsuario(new ChangePasswordDto
            {
                Id = 999,
                PasswordActual = "Actual@123",
                PasswordNueva = "Nueva@12345"
            });

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("Usuario no encontrado");
        }

        [Fact]
        public async Task ResetearPassword_ConUsuarioInexistente_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearUsuarioService(context);

            var resultado = await service.ResetearPassword(
                new ResetearPasswordDto { Id = 999, NuevaContraseña = "Temporal@1" }, usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("Usuario no encontrado");
        }

        // Un reseteo de contraseña (por un administrador) siempre deja la bandera en true,
        // a diferencia de un cambio voluntario: el usuario deberá cambiarla en su próximo login.
        [Fact]
        public async Task ResetearPassword_ConDatosValidos_MarcaPasswordComoTemporal()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var usuario = await SembrarUsuarioConPasswordAsync(context);
            usuario.ConfirmarCambioPassword();
            await context.SaveChangesAsync();
            var service = IntegrationTestFactory.CrearUsuarioService(context);

            var resultado = await service.ResetearPassword(
                new ResetearPasswordDto { Id = usuario.Id, NuevaContraseña = "Temporal@1" }, usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            usuario.DebeCambiarPassword.Should().BeTrue();
            PasswordHelper.Verify("Temporal@1", usuario.PasswordHash).Should().BeTrue();
        }
    }
}
