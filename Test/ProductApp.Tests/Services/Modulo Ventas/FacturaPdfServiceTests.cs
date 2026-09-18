using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ProductApp.Aplication.Services;
using ProductApp.Domian.Common.Enums.EnumsOrden;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using Xunit;

namespace ProductApp.Tests.Services.Modulo_Ventas
{
    public class FacturaPdfServiceTests
    {
        // NombreBlob usa el parámetro ordenId del método, no orden.Id, así que no hace
        // falta forzar el Id de la entidad (que EF asignaría después de persistir).
        private static Orden OrdenPagada()
        {
            var orden = new Orden(clienteId: 1, usuarioId: 1);
            orden.ActualizarTotal(100);
            orden.CambiarEstado(EstadoOrden.Pagada);
            return orden;
        }

        private static List<OrdenDetalle> UnDetalle() => new() { new OrdenDetalle(1, 2, 50m, ordenId: 1) };

        private static (FacturaPdfService Service,
            Mock<IOrdenRepository> OrdenRepo,
            Mock<IDetalleOrdenRepository> DetalleRepo,
            Mock<IPagoRepository> PagoRepo,
            Mock<IConfiguracionSistemaRepository> ConfigRepo,
            Mock<IGeneradorFacturaPdf> Generador,
            Mock<IAlmacenamientoFacturas> Almacenamiento) Crear()
        {
            var ordenRepo = new Mock<IOrdenRepository>();
            var detalleRepo = new Mock<IDetalleOrdenRepository>();
            var pagoRepo = new Mock<IPagoRepository>();
            var configRepo = new Mock<IConfiguracionSistemaRepository>();
            var generador = new Mock<IGeneradorFacturaPdf>();
            var almacenamiento = new Mock<IAlmacenamientoFacturas>();

            var service = new FacturaPdfService(
                ordenRepo.Object, detalleRepo.Object, pagoRepo.Object, configRepo.Object,
                generador.Object, almacenamiento.Object, NullLogger<FacturaPdfService>.Instance);

            return (service, ordenRepo, detalleRepo, pagoRepo, configRepo, generador, almacenamiento);
        }

        [Fact]
        public async Task GenerarYAlmacenarAsync_OrdenInexistente_DevuelveFailure()
        {
            var (service, ordenRepo, _, _, _, _, _) = Crear();
            ordenRepo.Setup(r => r.GetByIdConClienteAsync(It.IsAny<int>())).ReturnsAsync((Orden?)null);

            var resultado = await service.GenerarYAlmacenarAsync(7);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be("Orden no encontrada");
        }

        [Fact]
        public async Task GenerarYAlmacenarAsync_OrdenPendiente_DevuelveFailure()
        {
            var (service, ordenRepo, _, _, _, generador, _) = Crear();
            var orden = new Orden(clienteId: 1, usuarioId: 1);
            ordenRepo.Setup(r => r.GetByIdConClienteAsync(It.IsAny<int>())).ReturnsAsync(orden);

            var resultado = await service.GenerarYAlmacenarAsync(7);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("estado Pagada");
            generador.Verify(g => g.GenerarAsync(
                It.IsAny<Orden>(), It.IsAny<IReadOnlyList<OrdenDetalle>>(), It.IsAny<ConfiguracionSistema?>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task GenerarYAlmacenarAsync_OrdenCancelada_DevuelveFailure()
        {
            var (service, ordenRepo, _, _, _, _, _) = Crear();
            var orden = new Orden(clienteId: 1, usuarioId: 1);
            orden.CambiarEstado(EstadoOrden.Cancelada);
            ordenRepo.Setup(r => r.GetByIdConClienteAsync(It.IsAny<int>())).ReturnsAsync(orden);

            var resultado = await service.GenerarYAlmacenarAsync(7);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("estado Pagada");
        }

        [Fact]
        public async Task GenerarYAlmacenarAsync_OrdenPagadaSinDetalles_DevuelveFailure()
        {
            var (service, ordenRepo, detalleRepo, _, _, _, _) = Crear();
            ordenRepo.Setup(r => r.GetByIdConClienteAsync(It.IsAny<int>())).ReturnsAsync(OrdenPagada());
            detalleRepo.Setup(r => r.ObtenerPorOrdenIdAsync(It.IsAny<int>())).ReturnsAsync(new List<OrdenDetalle>());

            var resultado = await service.GenerarYAlmacenarAsync(7);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("no tiene productos");
        }

        // Regla del SDD 2.2: nunca se bloquea un cobro por configuración de empresa incompleta.
        [Fact]
        public async Task GenerarYAlmacenarAsync_SinConfiguracionDeEmpresa_GeneraLaFacturaIgual()
        {
            var (service, ordenRepo, detalleRepo, pagoRepo, configRepo, generador, almacenamiento) = Crear();
            ordenRepo.Setup(r => r.GetByIdConClienteAsync(It.IsAny<int>())).ReturnsAsync(OrdenPagada());
            detalleRepo.Setup(r => r.ObtenerPorOrdenIdAsync(It.IsAny<int>())).ReturnsAsync(UnDetalle());
            pagoRepo.Setup(r => r.ObtenerTotalPagadoPorOrdenAsync(It.IsAny<int>())).ReturnsAsync(100m);
            configRepo.Setup(r => r.ObtenerAsync()).ReturnsAsync((ConfiguracionSistema?)null);
            generador.Setup(g => g.GenerarAsync(
                    It.IsAny<Orden>(), It.IsAny<IReadOnlyList<OrdenDetalle>>(), null, It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new byte[] { 1, 2, 3 });
            almacenamiento.Setup(a => a.SubirAsync(It.IsAny<byte[]>(), "orden-1.pdf", It.IsAny<CancellationToken>()))
                .ReturnsAsync("https://blob/facturas/orden-1.pdf");

            var resultado = await service.GenerarYAlmacenarAsync(1);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data.Should().Be("https://blob/facturas/orden-1.pdf");
        }

        [Fact]
        public async Task GenerarYAlmacenarAsync_SiElStorageLanza_DevuelveFailureSinPropagar()
        {
            var (service, ordenRepo, detalleRepo, pagoRepo, configRepo, generador, almacenamiento) = Crear();
            ordenRepo.Setup(r => r.GetByIdConClienteAsync(It.IsAny<int>())).ReturnsAsync(OrdenPagada());
            detalleRepo.Setup(r => r.ObtenerPorOrdenIdAsync(It.IsAny<int>())).ReturnsAsync(UnDetalle());
            pagoRepo.Setup(r => r.ObtenerTotalPagadoPorOrdenAsync(It.IsAny<int>())).ReturnsAsync(100m);
            configRepo.Setup(r => r.ObtenerAsync()).ReturnsAsync((ConfiguracionSistema?)null);
            generador.Setup(g => g.GenerarAsync(
                    It.IsAny<Orden>(), It.IsAny<IReadOnlyList<OrdenDetalle>>(), It.IsAny<ConfiguracionSistema?>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new byte[] { 1 });
            almacenamiento.Setup(a => a.SubirAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("azurite caído"));

            var resultado = await service.GenerarYAlmacenarAsync(7);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be("No se pudo generar la factura PDF de la orden");
        }

        [Fact]
        public async Task GenerarYAlmacenarAsync_SiElRenderizadoLanza_DevuelveFailureSinPropagar()
        {
            var (service, ordenRepo, detalleRepo, pagoRepo, configRepo, generador, _) = Crear();
            ordenRepo.Setup(r => r.GetByIdConClienteAsync(It.IsAny<int>())).ReturnsAsync(OrdenPagada());
            detalleRepo.Setup(r => r.ObtenerPorOrdenIdAsync(It.IsAny<int>())).ReturnsAsync(UnDetalle());
            pagoRepo.Setup(r => r.ObtenerTotalPagadoPorOrdenAsync(It.IsAny<int>())).ReturnsAsync(100m);
            configRepo.Setup(r => r.ObtenerAsync()).ReturnsAsync((ConfiguracionSistema?)null);
            generador.Setup(g => g.GenerarAsync(
                    It.IsAny<Orden>(), It.IsAny<IReadOnlyList<OrdenDetalle>>(), It.IsAny<ConfiguracionSistema?>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("fuente no encontrada"));

            var resultado = await service.GenerarYAlmacenarAsync(7);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be("No se pudo generar la factura PDF de la orden");
        }

        [Fact]
        public async Task GenerarYAlmacenarAsync_UsaLaConvencionDeNombreDelSdd()
        {
            var (service, ordenRepo, detalleRepo, pagoRepo, configRepo, generador, almacenamiento) = Crear();
            ordenRepo.Setup(r => r.GetByIdConClienteAsync(It.IsAny<int>())).ReturnsAsync(OrdenPagada());
            detalleRepo.Setup(r => r.ObtenerPorOrdenIdAsync(It.IsAny<int>())).ReturnsAsync(UnDetalle());
            pagoRepo.Setup(r => r.ObtenerTotalPagadoPorOrdenAsync(It.IsAny<int>())).ReturnsAsync(100m);
            configRepo.Setup(r => r.ObtenerAsync()).ReturnsAsync((ConfiguracionSistema?)null);
            generador.Setup(g => g.GenerarAsync(
                    It.IsAny<Orden>(), It.IsAny<IReadOnlyList<OrdenDetalle>>(), It.IsAny<ConfiguracionSistema?>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new byte[] { 1 });
            almacenamiento.Setup(a => a.SubirAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("https://blob/facturas/orden-7.pdf");

            await service.GenerarYAlmacenarAsync(7);

            almacenamiento.Verify(a => a.SubirAsync(It.IsAny<byte[]>(), "orden-7.pdf", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GenerarYAlmacenarAsync_PasaElTotalPagadoAlGenerador()
        {
            var (service, ordenRepo, detalleRepo, pagoRepo, configRepo, generador, almacenamiento) = Crear();
            ordenRepo.Setup(r => r.GetByIdConClienteAsync(It.IsAny<int>())).ReturnsAsync(OrdenPagada());
            detalleRepo.Setup(r => r.ObtenerPorOrdenIdAsync(It.IsAny<int>())).ReturnsAsync(UnDetalle());
            pagoRepo.Setup(r => r.ObtenerTotalPagadoPorOrdenAsync(It.IsAny<int>())).ReturnsAsync(85m);
            configRepo.Setup(r => r.ObtenerAsync()).ReturnsAsync((ConfiguracionSistema?)null);
            generador.Setup(g => g.GenerarAsync(
                    It.IsAny<Orden>(), It.IsAny<IReadOnlyList<OrdenDetalle>>(), It.IsAny<ConfiguracionSistema?>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new byte[] { 1 });
            almacenamiento.Setup(a => a.SubirAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("https://blob/x.pdf");

            await service.GenerarYAlmacenarAsync(7);

            generador.Verify(g => g.GenerarAsync(
                It.IsAny<Orden>(), It.IsAny<IReadOnlyList<OrdenDetalle>>(), It.IsAny<ConfiguracionSistema?>(), 85m, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task ObtenerAsync_OrdenPendiente_DevuelveFailure()
        {
            var (service, ordenRepo, _, _, _, _, _) = Crear();
            ordenRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new Orden(clienteId: 1, usuarioId: 1));

            var resultado = await service.ObtenerAsync(7);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("órdenes pagadas");
        }

        [Fact]
        public async Task ObtenerAsync_OrdenEntregada_DevuelveLaFactura()
        {
            var (service, ordenRepo, _, _, _, _, almacenamiento) = Crear();
            var orden = OrdenPagada();
            orden.CambiarEstado(EstadoOrden.Entregada);
            ordenRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(orden);
            almacenamiento.Setup(a => a.DescargarAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new byte[] { 1, 2, 3 });

            var resultado = await service.ObtenerAsync(7);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
        }

        [Theory]
        [InlineData(null)]
        public async Task ObtenerAsync_SinArchivoEnElStorage_DevuelveFailure(byte[]? contenido)
        {
            var (service, ordenRepo, _, _, _, _, almacenamiento) = Crear();
            ordenRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(OrdenPagada());
            almacenamiento.Setup(a => a.DescargarAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(contenido);

            var resultado = await service.ObtenerAsync(7);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("no tiene una factura generada");
        }

        [Fact]
        public async Task ObtenerAsync_ConArchivoVacioEnElStorage_DevuelveFailure()
        {
            var (service, ordenRepo, _, _, _, _, almacenamiento) = Crear();
            ordenRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(OrdenPagada());
            almacenamiento.Setup(a => a.DescargarAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<byte>());

            var resultado = await service.ObtenerAsync(7);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("no tiene una factura generada");
        }
    }
}
