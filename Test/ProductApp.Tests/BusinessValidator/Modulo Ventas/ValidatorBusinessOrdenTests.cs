using FluentAssertions;
using Moq;
using ProductApp.Aplication.BusinessValidator.Modulo_Ventas;
using ProductApp.Domian.Common.Enums.EnumsOrden;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using Xunit;

namespace ProductApp.Tests.BusinessValidator.Modulo_Ventas
{
    public class ValidatorBusinessOrdenTests
    {
        private static Cliente CrearCliente()
            => new Cliente("Cliente Test", "001-0000000-1", "Calle Falsa 123", "cliente@test.com", "809-000-0000");

        [Fact]
        public async Task ValidarCrearOrdenAsync_ClienteNoExiste_DevuelveFailure()
        {
            var clienteRepoMock = new Mock<IClienteRepository>();
            clienteRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Cliente?)null);
            var validator = new ValidatorBusinessOrden(clienteRepoMock.Object);

            var resultado = await validator.ValidarCrearOrdenAsync(1);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("El cliente no existe");
        }

        [Fact]
        public async Task ValidarCrearOrdenAsync_ClienteInactivo_DevuelveFailure()
        {
            var cliente = CrearCliente();
            cliente.Desactivar();
            var clienteRepoMock = new Mock<IClienteRepository>();
            clienteRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(cliente);
            var validator = new ValidatorBusinessOrden(clienteRepoMock.Object);

            var resultado = await validator.ValidarCrearOrdenAsync(1);

            resultado.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task ValidarCrearOrdenAsync_ClienteActivo_DevuelveSuccess()
        {
            var cliente = CrearCliente();
            var clienteRepoMock = new Mock<IClienteRepository>();
            clienteRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(cliente);
            var validator = new ValidatorBusinessOrden(clienteRepoMock.Object);

            var resultado = await validator.ValidarCrearOrdenAsync(1);

            resultado.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task ValidarCambiarEstadoAsync_NuevoEstadoPagada_DevuelveFailure()
        {
            var validator = new ValidatorBusinessOrden(new Mock<IClienteRepository>().Object);

            var resultado = await validator.ValidarCambiarEstadoAsync(EstadoOrden.Pagada);

            resultado.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task ValidarCambiarEstadoAsync_NuevoEstadoProcesada_DevuelveFailure()
        {
            var validator = new ValidatorBusinessOrden(new Mock<IClienteRepository>().Object);

            var resultado = await validator.ValidarCambiarEstadoAsync(EstadoOrden.Procesada);

            resultado.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task ValidarCambiarEstadoAsync_NuevoEstadoCancelada_DevuelveSuccess()
        {
            var validator = new ValidatorBusinessOrden(new Mock<IClienteRepository>().Object);

            var resultado = await validator.ValidarCambiarEstadoAsync(EstadoOrden.Cancelada);

            resultado.IsSuccess.Should().BeTrue();
        }
    }
}
