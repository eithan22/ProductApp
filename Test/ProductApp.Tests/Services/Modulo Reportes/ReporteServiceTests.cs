using FluentAssertions;
using Moq;
using ProductApp.Aplication.Mappers.Modulo_Reportes;
using ProductApp.Aplication.Services;
using ProductApp.Domian.Interfaces;
using Xunit;

namespace ProductApp.Tests.Services.Modulo_Reportes
{
    public class ReporteServiceTests
    {
        private static (ReporteService Service, Mock<IReporteRepository> Repo, Mock<IInventarioRepository> Inventario) Crear()
        {
            var repo = new Mock<IReporteRepository>();
            var inventario = new Mock<IInventarioRepository>();
            return (new ReporteService(repo.Object, inventario.Object, new ReporteMapper()), repo, inventario);
        }

        private static string Texto(byte[] csv) => System.Text.Encoding.UTF8.GetString(csv);

        [Fact]
        public async Task ObtenerVentasPorFechaAsync_ConDesdeMayorQueHasta_DevuelveFailure()
        {
            var (service, _, _) = Crear();
            var hoy = DateTime.UtcNow;

            var resultado = await service.ObtenerVentasPorFechaAsync(hoy, hoy.AddDays(-1));

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("no puede ser mayor");
        }

        [Fact]
        public async Task ObtenerVentasPorFechaAsync_SinFechas_ConsultaLosUltimos30Dias()
        {
            var (service, repo, _) = Crear();
            DateTime desdeCapturado = default, hastaCapturado = default;
            repo.Setup(r => r.ObtenerVentasPorFechaAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback<DateTime, DateTime>((d, h) => { desdeCapturado = d; hastaCapturado = h; })
                .ReturnsAsync(new List<(DateTime, int, decimal)>());

            await service.ObtenerVentasPorFechaAsync(null, null);

            (hastaCapturado - desdeCapturado).TotalDays.Should().BeApproximately(30, 0.01);
        }

        [Fact]
        public async Task ObtenerVentasPorVendedorAsync_NoAdministrador_IgnoraElUsuarioIdPedidoYUsaElAutenticado()
        {
            var (service, repo, _) = Crear();
            repo.Setup(r => r.ObtenerVentasPorVendedorAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<(int, string, int, decimal)>());

            await service.ObtenerVentasPorVendedorAsync(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, usuarioId: 99, usuarioAutenticadoId: 7, esAdministrador: false);

            repo.Verify(r => r.ObtenerVentasPorVendedorAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 7), Times.Once);
        }

        [Fact]
        public async Task ObtenerVentasPorVendedorAsync_Administrador_RespetaElUsuarioIdPedido()
        {
            var (service, repo, _) = Crear();
            repo.Setup(r => r.ObtenerVentasPorVendedorAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<(int, string, int, decimal)>());

            await service.ObtenerVentasPorVendedorAsync(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, usuarioId: 99, usuarioAutenticadoId: 7, esAdministrador: true);

            repo.Verify(r => r.ObtenerVentasPorVendedorAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 99), Times.Once);
        }

        [Fact]
        public async Task ObtenerVentasPorVendedorAsync_AdministradorSinUsuarioId_ConsultaTodos()
        {
            var (service, repo, _) = Crear();
            repo.Setup(r => r.ObtenerVentasPorVendedorAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<(int, string, int, decimal)>());

            await service.ObtenerVentasPorVendedorAsync(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, usuarioId: null, usuarioAutenticadoId: 7, esAdministrador: true);

            repo.Verify(r => r.ObtenerVentasPorVendedorAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), (int?)null), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-3)]
        public async Task ObtenerProductosMasVendidosAsync_ConTopNoPositivo_DevuelveFailure(int top)
        {
            var (service, repo, _) = Crear();

            var resultado = await service.ObtenerProductosMasVendidosAsync(null, null, top);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("'top'");
            repo.Verify(r => r.ObtenerProductosMasVendidosAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task ObtenerIngresosTotalesAsync_SinPagos_DevuelveTicketPromedioCero()
        {
            var (service, repo, _) = Crear();
            repo.Setup(r => r.ObtenerIngresosTotalesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync((0m, 0, 0));

            var resultado = await service.ObtenerIngresosTotalesAsync(null, null);

            resultado.Data!.TicketPromedio.Should().Be(0);
        }

        // M14 (auditoría de seguridad): el ticket promedio se calcula por ORDEN, no por pago —
        // 4 pagos repartidos en solo 2 órdenes (dos abonos parciales cada una) dan un ticket
        // promedio de 500, no de 250 como daría dividir entre la cantidad de pagos.
        [Fact]
        public async Task ObtenerIngresosTotalesAsync_ConPagosParciales_CalculaElTicketPromedioPorOrdenNoPorPago()
        {
            var (service, repo, _) = Crear();
            repo.Setup(r => r.ObtenerIngresosTotalesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync((1000m, 4, 2));

            var resultado = await service.ObtenerIngresosTotalesAsync(null, null);

            resultado.Data!.TicketPromedio.Should().Be(500m);
        }

        [Fact]
        public async Task ExportarVentasPorFechaCsvAsync_EscribeBomUtf8YSeparadorPuntoYComa()
        {
            var (service, repo, _) = Crear();
            repo.Setup(r => r.ObtenerVentasPorFechaAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<(DateTime, int, decimal)> { (DateTime.UtcNow, 3, 1000m) });

            var resultado = await service.ExportarVentasPorFechaCsvAsync(null, null);

            resultado.Data![0].Should().Be(0xEF);
            resultado.Data[1].Should().Be(0xBB);
            resultado.Data[2].Should().Be(0xBF);
            Texto(resultado.Data).Should().Contain("Fecha;Cantidad de órdenes;Total");
        }

        [Fact]
        public async Task ExportarVentasPorProductoCsvAsync_ConNombreQueTraePuntoYComa_LoEncierraEntreComillas()
        {
            var (service, repo, _) = Crear();
            repo.Setup(r => r.ObtenerVentasPorProductoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<(int, string, int, decimal)> { (1, "Taladro; industrial", 2, 500m) });

            var resultado = await service.ExportarVentasPorProductoCsvAsync(null, null);

            Texto(resultado.Data!).Should().Contain("\"Taladro; industrial\"");
        }

        [Fact]
        public async Task ExportarVentasPorProductoCsvAsync_ConNombreQueTraeComillas_LasDuplica()
        {
            var (service, repo, _) = Crear();
            repo.Setup(r => r.ObtenerVentasPorProductoAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<(int, string, int, decimal)> { (1, "Tubo \"A\"", 2, 500m) });

            var resultado = await service.ExportarVentasPorProductoCsvAsync(null, null);

            Texto(resultado.Data!).Should().Contain("\"Tubo \"\"A\"\"\"");
        }

        [Fact]
        public async Task ExportarVentasPorFechaCsvAsync_EscribeLosMontosConPuntoDecimalYDosDecimales()
        {
            var (service, repo, _) = Crear();
            repo.Setup(r => r.ObtenerVentasPorFechaAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync(new List<(DateTime, int, decimal)> { (DateTime.UtcNow, 1, 1234.5m) });

            var resultado = await service.ExportarVentasPorFechaCsvAsync(null, null);

            Texto(resultado.Data!).Should().Contain("1234.50");
        }

        [Fact]
        public async Task ExportarProductosMasVendidosCsvAsync_ConTopInvalido_PropagaElFailureSinGenerarArchivo()
        {
            var (service, _, _) = Crear();

            var resultado = await service.ExportarProductosMasVendidosCsvAsync(null, null, top: 0);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Data.Should().BeNull();
        }

        [Fact]
        public async Task ExportarIngresosTotalesCsvAsync_GeneraUnaSolaFilaDeDatos()
        {
            var (service, repo, _) = Crear();
            repo.Setup(r => r.ObtenerIngresosTotalesAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .ReturnsAsync((500m, 2, 2));

            var resultado = await service.ExportarIngresosTotalesCsvAsync(null, null);

            var lineas = Texto(resultado.Data!).Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
            lineas.Should().HaveCount(2);
        }

        [Fact]
        public async Task ExportarVentasPorVendedorCsvAsync_NoAdministrador_TambienFiltraPorElUsuarioAutenticado()
        {
            var (service, repo, _) = Crear();
            repo.Setup(r => r.ObtenerVentasPorVendedorAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
                .ReturnsAsync(new List<(int, string, int, decimal)>());

            await service.ExportarVentasPorVendedorCsvAsync(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow, usuarioId: 99, usuarioAutenticadoId: 7, esAdministrador: false);

            repo.Verify(r => r.ObtenerVentasPorVendedorAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), 7), Times.Once);
        }
    }
}
