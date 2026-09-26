using FluentAssertions;
using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto;
using ProductApp.Domian.Common.Enums.EnumsUsuario;
using ProductApp.Domian.Common.Legal;
using ProductApp.Domian.Entitis;
using Xunit;

namespace ProductApp.Tests.Integration
{
    // Cubre el registro de la aceptación de los documentos legales: qué versión quedó grabada,
    // cuándo, y qué pasa cuando el cliente manda una versión que ya no es la vigente.
    public class UsuarioServiceAceptacionDocumentosLegalesTests
    {
        private static async Task<Usuario> SembrarUsuarioAsync(
            Infraesctructura.Persistencia.Contex.AppDbContext context)
        {
            var usuario = new Usuario("Usuario Test", "usuario@test.com", "usuario.test", RolUsuario.Vendedor);
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();
            return usuario;
        }

        [Fact]
        public async Task RegistrarAceptacionDocumentosLegalesAsync_ConLaVersionVigente_GrabaVersionYFecha()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var usuario = await SembrarUsuarioAsync(context);
            var service = IntegrationTestFactory.CrearUsuarioService(context);
            var antes = DateTime.UtcNow;

            var resultado = await service.RegistrarAceptacionDocumentosLegalesAsync(
                usuario.Id, new AceptarDocumentosLegalesDto { Version = DocumentosLegales.VersionVigente });

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            usuario.VersionDocumentosLegalesAceptada.Should().Be(DocumentosLegales.VersionVigente);
            usuario.FechaAceptacionDocumentosLegales.Should().NotBeNull();
            usuario.FechaAceptacionDocumentosLegales!.Value.Should().BeOnOrAfter(antes);
            usuario.DebeAceptarDocumentosLegales.Should().BeFalse();
        }

        // Si se publicó una versión nueva mientras el usuario leía, lo que aceptó no es lo que
        // rige: la aceptación se rechaza en vez de darse por buena.
        [Fact]
        public async Task RegistrarAceptacionDocumentosLegalesAsync_ConUnaVersionQueNoEsLaVigente_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var usuario = await SembrarUsuarioAsync(context);
            var service = IntegrationTestFactory.CrearUsuarioService(context);

            var resultado = await service.RegistrarAceptacionDocumentosLegalesAsync(
                usuario.Id, new AceptarDocumentosLegalesDto { Version = "1.0" });

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("cambiaron mientras los leías");
            usuario.VersionDocumentosLegalesAceptada.Should().BeNull();
            usuario.DebeAceptarDocumentosLegales.Should().BeTrue();
        }

        [Fact]
        public async Task RegistrarAceptacionDocumentosLegalesAsync_ConVersionVacia_DevuelveFailureDelValidador()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var usuario = await SembrarUsuarioAsync(context);
            var service = IntegrationTestFactory.CrearUsuarioService(context);

            var resultado = await service.RegistrarAceptacionDocumentosLegalesAsync(
                usuario.Id, new AceptarDocumentosLegalesDto { Version = string.Empty });

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("La versión de los documentos es requerida.");
        }

        [Fact]
        public async Task RegistrarAceptacionDocumentosLegalesAsync_ConUsuarioInexistente_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearUsuarioService(context);

            var resultado = await service.RegistrarAceptacionDocumentosLegalesAsync(
                999, new AceptarDocumentosLegalesDto { Version = DocumentosLegales.VersionVigente });

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("Usuario no encontrado");
        }

        // La fecha que vale como prueba es la de la PRIMERA aceptación: un segundo envío (doble
        // clic, F5 sobre el POST) no la puede sobrescribir.
        [Fact]
        public async Task RegistrarAceptacionDocumentosLegalesAsync_DosVecesLaMismaVersion_ConservaLaFechaOriginal()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var usuario = await SembrarUsuarioAsync(context);
            var service = IntegrationTestFactory.CrearUsuarioService(context);
            var dto = new AceptarDocumentosLegalesDto { Version = DocumentosLegales.VersionVigente };

            await service.RegistrarAceptacionDocumentosLegalesAsync(usuario.Id, dto);
            var fechaOriginal = usuario.FechaAceptacionDocumentosLegales;

            var resultado = await service.RegistrarAceptacionDocumentosLegalesAsync(usuario.Id, dto);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            usuario.FechaAceptacionDocumentosLegales.Should().Be(fechaOriginal);
        }
    }
}
