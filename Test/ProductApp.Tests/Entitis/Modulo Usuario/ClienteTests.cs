using FluentAssertions;
using ProductApp.Domian.Common.Enums.EnumsCliente;
using ProductApp.Domian.Common.Exceptions;
using ProductApp.Domian.Entitis;
using Xunit;

namespace ProductApp.Tests.Entitis
{
    public class ClienteTests
    {
        private static Cliente CrearCliente()
            => new Cliente("Cliente Test", "001-0000000-1", "Calle Falsa 123", "cliente@test.com", "809-000-0000");

        [Fact]
        public void Desactivar_EnClienteActivo_LoDejaInactivo()
        {
            var cliente = CrearCliente();

            cliente.Desactivar();

            cliente.Estado.Should().Be(EstadoCliente.Inactivo);
        }

        [Fact]
        public void Desactivar_EnClienteYaInactivo_LanzaEstadoInvalidoException()
        {
            var cliente = CrearCliente();
            cliente.Desactivar();

            var accion = () => cliente.Desactivar();

            accion.Should().Throw<EstadoInvalidoException>();
        }

        [Fact]
        public void Activar_EnClienteInactivo_LoDejaActivo()
        {
            var cliente = CrearCliente();
            cliente.Desactivar();

            cliente.Activar();

            cliente.Estado.Should().Be(EstadoCliente.Activo);
        }

        [Fact]
        public void Activar_EnClienteYaActivo_LanzaEstadoInvalidoException()
        {
            var cliente = CrearCliente();

            var accion = () => cliente.Activar();

            accion.Should().Throw<EstadoInvalidoException>();
        }
    }
}
