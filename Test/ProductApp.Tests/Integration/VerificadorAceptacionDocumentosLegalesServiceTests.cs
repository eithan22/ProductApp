using FluentAssertions;
using ProductApp.Domian.Common.Enums.EnumsUsuario;
using ProductApp.Domian.Common.Legal;
using ProductApp.Domian.Entitis;
using Xunit;

namespace ProductApp.Tests.Integration
{
    // Cubre el gate de aceptación de los documentos legales que RequiereAceptacionDocumentosLegalesFilter
    // usa para cortar con 403 las peticiones de un usuario que todavía no aceptó la versión vigente
    // de los Términos y la Política de Privacidad. El filtro no tiene reglas: todas viven acá.
    public class VerificadorAceptacionDocumentosLegalesServiceTests
    {
        private const string MensajeEsperado = "Debe aceptar los Términos de Servicio";

        private static async Task<Usuario> SembrarUsuarioAsync(
            ProductApp.Infraesctructura.Persistencia.Contex.AppDbContext context,
            string? versionAceptada = null)
        {
            var usuario = new Usuario("Usuario Test", "usuario@test.com", "usuario.test", RolUsuario.Vendedor);

            if (versionAceptada != null)
                usuario.RegistrarAceptacionDocumentosLegales(versionAceptada);

            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();
            return usuario;
        }

        [Fact]
        public async Task VerificarAceptacionAsync_ConLaVersionVigenteAceptada_DevuelveSuccess()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var usuario = await SembrarUsuarioAsync(context, DocumentosLegales.VersionVigente);
            var service = IntegrationTestFactory.CrearVerificadorAceptacionDocumentosLegalesService(context);

            var resultado = await service.VerificarAceptacionAsync(usuario.Id);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
        }

        // El caso de todo usuario nuevo y de todos los que ya existían cuando se agregó el
        // registro: la columna queda en null y hay que pedirles la aceptación.
        [Fact]
        public async Task VerificarAceptacionAsync_SinAceptacionRegistrada_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var usuario = await SembrarUsuarioAsync(context);
            var service = IntegrationTestFactory.CrearVerificadorAceptacionDocumentosLegalesService(context);

            var resultado = await service.VerificarAceptacionAsync(usuario.Id);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain(MensajeEsperado);
        }

        // Es lo que ocurre cuando se publica una versión nueva: la aceptación anterior deja de
        // valer y el usuario vuelve a pasar por la pantalla.
        [Fact]
        public async Task VerificarAceptacionAsync_ConUnaVersionVieja_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var usuario = await SembrarUsuarioAsync(context, "1.0");
            var service = IntegrationTestFactory.CrearVerificadorAceptacionDocumentosLegalesService(context);

            var resultado = await service.VerificarAceptacionAsync(usuario.Id);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain(MensajeEsperado);
        }

        // La baja de usuario es lógica (EstaEliminado), así que la fila sigue ahí: la consulta
        // tiene que descartarla igual que el resto de los métodos del repositorio.
        [Fact]
        public async Task VerificarAceptacionAsync_ConUsuarioEliminado_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var usuario = await SembrarUsuarioAsync(context, DocumentosLegales.VersionVigente);
            usuario.Eliminar();
            await context.SaveChangesAsync();
            var service = IntegrationTestFactory.CrearVerificadorAceptacionDocumentosLegalesService(context);

            var resultado = await service.VerificarAceptacionAsync(usuario.Id);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain(MensajeEsperado);
        }

        [Fact]
        public async Task VerificarAceptacionAsync_ConUsuarioInexistente_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearVerificadorAceptacionDocumentosLegalesService(context);

            var resultado = await service.VerificarAceptacionAsync(999);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain(MensajeEsperado);
        }

        // Es lo que recibe el servicio cuando el filtro no pudo parsear el claim del id.
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task VerificarAceptacionAsync_ConIdInvalido_DevuelveFailure(int usuarioId)
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearVerificadorAceptacionDocumentosLegalesService(context);

            var resultado = await service.VerificarAceptacionAsync(usuarioId);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain(MensajeEsperado);
        }
    }
}
