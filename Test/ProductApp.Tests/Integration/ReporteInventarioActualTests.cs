using FluentAssertions;
using Xunit;

namespace ProductApp.Tests.Integration
{
    // A diferencia de ReporteServiceTests (con mocks), este va acá porque
    // ReporteMapper.MapToInventarioActualDto lee inventario.Producto, que tiene `private set`
    // y solo lo carga EF real (GetAllConProductoAsync hace el Include). Con un Mock puro de
    // IInventarioRepository ese campo quedaría null y reventaría con NullReferenceException.
    public class ReporteInventarioActualTests
    {
        [Fact]
        public async Task ObtenerInventarioActualAsync_MarcaStockBajoCuandoLaCantidadNoSuperaLaMinima()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            // SembrarProductoConInventarioAsync fija la cantidad mínima en 1.
            await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 1);
            await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 10);
            var service = IntegrationTestFactory.CrearReporteService(context);

            var resultado = await service.ObtenerInventarioActualAsync();

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data.Should().HaveCount(2);
            resultado.Data!.Single(i => i.CantidadActual == 1).StockBajo.Should().BeTrue();
            resultado.Data!.Single(i => i.CantidadActual == 10).StockBajo.Should().BeFalse();
        }

        [Fact]
        public async Task ExportarInventarioActualCsvAsync_EscribeSiONoEnLaColumnaStockBajo()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 1);
            await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 10);
            var service = IntegrationTestFactory.CrearReporteService(context);

            var resultado = await service.ExportarInventarioActualCsvAsync();

            var texto = System.Text.Encoding.UTF8.GetString(resultado.Data!);
            texto.Should().Contain(";Sí");
            texto.Should().Contain(";No");
        }
    }
}
