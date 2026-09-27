using FluentAssertions;
using ProductApp.Domian.Common.Enums.EnumsProveedor;
using ProductApp.Domian.Common.Exceptions;
using ProductApp.Domian.Entitis;
using Xunit;

namespace ProductApp.Tests.Entitis
{
    public class ProveedorTests
    {
        private static Proveedor CrearProveedor()
            => new Proveedor("Proveedor Test", "8090000000", "proveedor@test.com", "Av. Principal 1");

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_ConNombreVacio_LanzaValidacionDominioException(string nombre)
        {
            var accion = () => new Proveedor(nombre, "8090000000", "p@test.com", "Calle 1");

            accion.Should().Throw<ValidacionDominioException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_ConTelefonoVacio_LanzaValidacionDominioException(string telefono)
        {
            var accion = () => new Proveedor("Proveedor", telefono, "p@test.com", "Calle 1");

            accion.Should().Throw<ValidacionDominioException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_ConCorreoVacio_LanzaValidacionDominioException(string correo)
        {
            var accion = () => new Proveedor("Proveedor", "8090000000", correo, "Calle 1");

            accion.Should().Throw<ValidacionDominioException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_ConDireccionVacia_LanzaValidacionDominioException(string direccion)
        {
            var accion = () => new Proveedor("Proveedor", "8090000000", "p@test.com", direccion);

            accion.Should().Throw<ValidacionDominioException>();
        }

        [Fact]
        public void Constructor_ConDatosValidos_NaceActivo()
        {
            var proveedor = CrearProveedor();

            proveedor.Estado.Should().Be(EstadoProveedor.Activo);
        }

        [Fact]
        public void Desactivar_EnProveedorActivo_LoDejaInactivo()
        {
            var proveedor = CrearProveedor();

            proveedor.Desactivar();

            proveedor.Estado.Should().Be(EstadoProveedor.Inactivo);
        }

        // La baja de un proveedor es por Estado, NO por el soft delete de BaseEntity.
        // Esto importa: ExisteAsync filtra por EstaEliminado, así que un proveedor
        // desactivado sigue ocupando su nombre y su correo.
        [Fact]
        public void Desactivar_NoMarcaLaEntidadComoEliminada()
        {
            var proveedor = CrearProveedor();

            proveedor.Desactivar();

            proveedor.EstaEliminado.Should().BeFalse();
        }

        [Fact]
        public void Desactivar_EnProveedorYaInactivo_LanzaEstadoInvalidoException()
        {
            var proveedor = CrearProveedor();
            proveedor.Desactivar();

            var accion = () => proveedor.Desactivar();

            accion.Should().Throw<EstadoInvalidoException>();
        }

        [Fact]
        public void Activar_EnProveedorInactivo_LoDejaActivo()
        {
            var proveedor = CrearProveedor();
            proveedor.Desactivar();

            proveedor.Activar();

            proveedor.Estado.Should().Be(EstadoProveedor.Activo);
        }

        [Fact]
        public void Activar_EnProveedorYaActivo_LanzaEstadoInvalidoException()
        {
            var proveedor = CrearProveedor();

            var accion = () => proveedor.Activar();

            accion.Should().Throw<EstadoInvalidoException>();
        }

        [Fact]
        public void CambiarYvalidarCorreo_ConValorValido_ActualizaElCorreo()
        {
            var proveedor = CrearProveedor();

            proveedor.CambiarYvalidarCorreo("nuevo@test.com");

            proveedor.Correo.Should().Be("nuevo@test.com");
        }

        [Fact]
        public void CambiarYvalidarNombre_ConValorVacio_NoMutaElNombreAnterior()
        {
            var proveedor = CrearProveedor();

            var accion = () => proveedor.CambiarYvalidarNombre("  ");

            accion.Should().Throw<ValidacionDominioException>();
            proveedor.Nombre.Should().Be("Proveedor Test");
        }
    }
}
