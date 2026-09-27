using FluentAssertions;
using ProductApp.Domian.Common.Enums.EnumsUsuario;
using ProductApp.Domian.Common.Exceptions;
using ProductApp.Domian.Common.Legal;
using ProductApp.Domian.Entitis;
using Xunit;

namespace ProductApp.Tests.Entitis
{
    public class UsuarioTests
    {
        private static Usuario CrearUsuario()
            => new Usuario("Usuario Test", "usuario@test.com", "usuario.test", RolUsuario.Vendedor);

        [Fact]
        public void MarcarPasswordComoTemporal_DejaDebeCambiarPasswordEnTrue()
        {
            var usuario = CrearUsuario();

            usuario.MarcarPasswordComoTemporal();

            usuario.DebeCambiarPassword.Should().BeTrue();
        }

        [Fact]
        public void ConfirmarCambioPassword_DejaDebeCambiarPasswordEnFalse()
        {
            var usuario = CrearUsuario();
            usuario.MarcarPasswordComoTemporal();

            usuario.ConfirmarCambioPassword();

            usuario.DebeCambiarPassword.Should().BeFalse();
        }

        // Un usuario recién creado nunca aceptó nada: la columna queda en null y el gate lo tiene
        // que detener. Es también el estado de los usuarios que ya existían antes de la migración.
        [Fact]
        public void UsuarioNuevo_DebeAceptarDocumentosLegales()
        {
            var usuario = CrearUsuario();

            usuario.VersionDocumentosLegalesAceptada.Should().BeNull();
            usuario.FechaAceptacionDocumentosLegales.Should().BeNull();
            usuario.DebeAceptarDocumentosLegales.Should().BeTrue();
        }

        [Fact]
        public void RegistrarAceptacionDocumentosLegales_ConLaVersionVigente_DejaDeExigirAceptacion()
        {
            var usuario = CrearUsuario();

            usuario.RegistrarAceptacionDocumentosLegales(DocumentosLegales.VersionVigente);

            usuario.VersionDocumentosLegalesAceptada.Should().Be(DocumentosLegales.VersionVigente);
            usuario.FechaAceptacionDocumentosLegales.Should().NotBeNull();
            usuario.DebeAceptarDocumentosLegales.Should().BeFalse();
        }

        // Aceptar una versión vieja no habilita nada: el gate compara contra la vigente.
        [Fact]
        public void RegistrarAceptacionDocumentosLegales_ConUnaVersionVieja_SigueExigiendoAceptacion()
        {
            var usuario = CrearUsuario();

            usuario.RegistrarAceptacionDocumentosLegales("1.0");

            usuario.VersionDocumentosLegalesAceptada.Should().Be("1.0");
            usuario.DebeAceptarDocumentosLegales.Should().BeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void RegistrarAceptacionDocumentosLegales_ConVersionVacia_LanzaValidacionDominioException(string version)
        {
            var usuario = CrearUsuario();

            var accion = () => usuario.RegistrarAceptacionDocumentosLegales(version);

            accion.Should().Throw<ValidacionDominioException>();
        }

        [Fact]
        public void RegistrarAceptacionDocumentosLegales_ConVersionDemasiadoLarga_LanzaValidacionDominioException()
        {
            var usuario = CrearUsuario();
            var versionLarga = new string('9', Usuario.LargoMaximoVersionDocumentosLegales + 1);

            var accion = () => usuario.RegistrarAceptacionDocumentosLegales(versionLarga);

            accion.Should().Throw<ValidacionDominioException>();
        }

        // La fecha de la primera aceptación es la prueba: un segundo registro de la misma versión
        // no la toca.
        [Fact]
        public void RegistrarAceptacionDocumentosLegales_DosVecesLaMismaVersion_ConservaLaFechaOriginal()
        {
            var usuario = CrearUsuario();
            usuario.RegistrarAceptacionDocumentosLegales(DocumentosLegales.VersionVigente);
            var fechaOriginal = usuario.FechaAceptacionDocumentosLegales;

            usuario.RegistrarAceptacionDocumentosLegales(DocumentosLegales.VersionVigente);

            usuario.FechaAceptacionDocumentosLegales.Should().Be(fechaOriginal);
        }

        [Fact]
        public void Desactivar_EnUsuarioActivo_LoDejaInactivo()
        {
            var usuario = CrearUsuario();

            usuario.Desactivar();

            usuario.EstadoUsuario.Should().Be(EstadoUsuario.Inactivo);
        }

        [Fact]
        public void Desactivar_EnUsuarioYaInactivo_LanzaEstadoInvalidoException()
        {
            var usuario = CrearUsuario();
            usuario.Desactivar();

            var accion = () => usuario.Desactivar();

            accion.Should().Throw<EstadoInvalidoException>();
        }

        [Fact]
        public void Activar_EnUsuarioInactivo_LoDejaActivo()
        {
            var usuario = CrearUsuario();
            usuario.Desactivar();

            usuario.Activar();

            usuario.EstadoUsuario.Should().Be(EstadoUsuario.Activo);
        }

        [Fact]
        public void Activar_EnUsuarioYaActivo_LanzaEstadoInvalidoException()
        {
            var usuario = CrearUsuario();

            var accion = () => usuario.Activar();

            accion.Should().Throw<EstadoInvalidoException>();
        }
    }
}
