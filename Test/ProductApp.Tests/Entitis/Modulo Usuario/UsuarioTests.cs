using FluentAssertions;
using ProductApp.Domian.Common.Enums.EnumsUsuario;
using ProductApp.Domian.Common.Exceptions;
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
