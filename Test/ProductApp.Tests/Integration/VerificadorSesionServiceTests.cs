using FluentAssertions;
using ProductApp.Domian.Common.Enums.EnumsUsuario;
using ProductApp.Domian.Entitis;
using Xunit;

namespace ProductApp.Tests.Integration
{
    // Cubre la re-verificación de sesión que VerificarSesionVigenteFilter usa para cortar con
    // 401 las peticiones de un token cuyo usuario ya fue desactivado, eliminado o cambió de rol
    // (M4 de la auditoría). El filtro en sí no tiene reglas: todas viven acá.
    public class VerificadorSesionServiceTests
    {
        private const string MensajeEsperado = "Su sesión ya no es válida";

        private static async Task<Usuario> SembrarUsuarioAsync(
            ProductApp.Infraesctructura.Persistencia.Contex.AppDbContext context,
            RolUsuario rol = RolUsuario.Vendedor)
        {
            var usuario = new Usuario("Usuario Test", "usuario@test.com", "usuario.test", rol);
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();
            return usuario;
        }

        [Fact]
        public async Task VerificarSesionAsync_ConUsuarioActivoYRolCoincidente_DevuelveSuccess()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var usuario = await SembrarUsuarioAsync(context);
            var service = IntegrationTestFactory.CrearVerificadorSesionService(context);

            var resultado = await service.VerificarSesionAsync(usuario.Id, "Vendedor");

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
        }

        // El claim lo escribe AuthServices con RolUsuario.ToString(), pero la comparación no
        // depende de la caja: mismo criterio que Enum.Parse(..., true) en UsuarioService.CambiarRol.
        [Fact]
        public async Task VerificarSesionAsync_ConRolDelTokenEnOtraCaja_DevuelveSuccess()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var usuario = await SembrarUsuarioAsync(context, RolUsuario.Administrador);
            var service = IntegrationTestFactory.CrearVerificadorSesionService(context);

            var resultado = await service.VerificarSesionAsync(usuario.Id, "administrador");

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
        }

        // Este es el caso que M4 dejaba abierto: el token seguía valiendo hasta vencer.
        [Fact]
        public async Task VerificarSesionAsync_ConUsuarioDesactivado_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var usuario = await SembrarUsuarioAsync(context);
            usuario.Desactivar();
            await context.SaveChangesAsync();
            var service = IntegrationTestFactory.CrearVerificadorSesionService(context);

            var resultado = await service.VerificarSesionAsync(usuario.Id, "Vendedor");

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain(MensajeEsperado);
        }

        // La baja de usuario es lógica (EstaEliminado), así que la fila sigue ahí: la consulta
        // tiene que descartarla igual que el resto de los métodos del repositorio.
        [Fact]
        public async Task VerificarSesionAsync_ConUsuarioEliminado_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var usuario = await SembrarUsuarioAsync(context);
            usuario.Eliminar();
            await context.SaveChangesAsync();
            var service = IntegrationTestFactory.CrearVerificadorSesionService(context);

            var resultado = await service.VerificarSesionAsync(usuario.Id, "Vendedor");

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain(MensajeEsperado);
        }

        [Fact]
        public async Task VerificarSesionAsync_ConUsuarioInexistente_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearVerificadorSesionService(context);

            var resultado = await service.VerificarSesionAsync(999, "Vendedor");

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain(MensajeEsperado);
        }

        // Escalada de privilegios: el token dice Administrador porque se emitió antes de la
        // degradación. [Authorize(Roles = "Administrador")] lo dejaría pasar; esto lo corta.
        [Fact]
        public async Task VerificarSesionAsync_ConRolDistintoAlDelToken_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var usuario = await SembrarUsuarioAsync(context, RolUsuario.Vendedor);
            var service = IntegrationTestFactory.CrearVerificadorSesionService(context);

            var resultado = await service.VerificarSesionAsync(usuario.Id, "Administrador");

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain(MensajeEsperado);
        }

        // Token sin claim de rol, vacío, o con un rol que ya no existe en el enum: se rechaza,
        // no se asume el menor privilegio.
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("Gerente")]
        public async Task VerificarSesionAsync_ConRolDelTokenIlegible_DevuelveFailure(string? rolEnElToken)
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var usuario = await SembrarUsuarioAsync(context);
            var service = IntegrationTestFactory.CrearVerificadorSesionService(context);

            var resultado = await service.VerificarSesionAsync(usuario.Id, rolEnElToken);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain(MensajeEsperado);
        }

        // Es lo que recibe el servicio cuando el filtro no pudo parsear el claim del id.
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task VerificarSesionAsync_ConIdInvalido_DevuelveFailure(int usuarioId)
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearVerificadorSesionService(context);

            var resultado = await service.VerificarSesionAsync(usuarioId, "Vendedor");

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain(MensajeEsperado);
        }
    }
}
