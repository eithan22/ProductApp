using FluentAssertions;
using ProductApp.Domian.Common.Exceptions;
using ProductApp.Domian.Entitis;
using Xunit;

namespace ProductApp.Tests.Entitis
{
    public class InventarioTests
    {
        private static Inventario CrearInventario(int cantidadActual = 10, int cantidadMinima = 5, int productoId = 1)
            => new Inventario(cantidadActual, cantidadMinima, productoId);

        [Fact]
        public void Constructor_ConCantidadActualNegativa_LanzaValidacionDominioException()
        {
            var accion = () => CrearInventario(cantidadActual: -1);

            accion.Should().Throw<ValidacionDominioException>();
        }

        [Fact]
        public void Constructor_ConCantidadMinimaNegativa_LanzaValidacionDominioException()
        {
            var accion = () => CrearInventario(cantidadMinima: -1);

            accion.Should().Throw<ValidacionDominioException>();
        }

        [Fact]
        public void EsStockBajo_ConCantidadActualIgualALaMinima_DevuelveTrue()
        {
            var inventario = CrearInventario(cantidadActual: 5, cantidadMinima: 5);

            inventario.EsStockBajo().Should().BeTrue();
        }

        [Fact]
        public void EsStockBajo_ConCantidadActualMayorALaMinima_DevuelveFalse()
        {
            var inventario = CrearInventario(cantidadActual: 10, cantidadMinima: 5);

            inventario.EsStockBajo().Should().BeFalse();
        }

        [Fact]
        public void AjustarStock_ConValorNegativo_LanzaValidacionDominioException()
        {
            var inventario = CrearInventario();

            var accion = () => inventario.AjustarStock(-5);

            accion.Should().Throw<ValidacionDominioException>();
        }

        [Fact]
        public void AjustarStockMinimo_ConValorNegativo_LanzaValidacionDominioException()
        {
            var inventario = CrearInventario();

            var accion = () => inventario.AjustarStockMinimo(-1);

            accion.Should().Throw<ValidacionDominioException>();
        }

        [Fact]
        public void RegistrarSalidaStock_ConCantidadMayorALaDisponible_LanzaValidacionDominioExceptionConMensajeDeStockInsuficiente()
        {
            var inventario = CrearInventario(cantidadActual: 5, cantidadMinima: 1);

            var accion = () => inventario.RegistrarSalidaStock(10);

            accion.Should().Throw<ValidacionDominioException>()
                .WithMessage("*Stock insuficiente*");
        }

        [Fact]
        public void RegistrarSalidaStock_ConCantidadValida_DescuentaLaCantidadActual()
        {
            var inventario = CrearInventario(cantidadActual: 10, cantidadMinima: 1);

            inventario.RegistrarSalidaStock(4);

            inventario.CantidadActual.Should().Be(6);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void RegistrarEntradaStock_ConCantidadCeroONegativa_LanzaValidacionDominioException(int cantidad)
        {
            var inventario = CrearInventario();

            var accion = () => inventario.RegistrarEntradaStock(cantidad);

            accion.Should().Throw<ValidacionDominioException>();
        }

        // --- Entrada de stock: camino feliz (antes sin cobertura) ---

        [Fact]
        public void RegistrarEntradaStock_ConCantidadValida_SumaALaCantidadActual()
        {
            var inventario = CrearInventario(cantidadActual: 10, cantidadMinima: 5);

            inventario.RegistrarEntradaStock(7);

            inventario.CantidadActual.Should().Be(17);
        }

        [Fact]
        public void RegistrarEntradaStock_SobreUnInventarioEnCero_DejaExactamenteLaCantidadIngresada()
        {
            var inventario = CrearInventario(cantidadActual: 0, cantidadMinima: 5);

            inventario.RegistrarEntradaStock(3);

            inventario.CantidadActual.Should().Be(3);
        }

        [Fact]
        public void RegistrarEntradaStock_ConLaCantidadMinimaValida_SumaUnaUnidad()
        {
            var inventario = CrearInventario(cantidadActual: 10, cantidadMinima: 5);

            inventario.RegistrarEntradaStock(1);

            inventario.CantidadActual.Should().Be(11);
        }

        [Fact]
        public void RegistrarEntradaStock_VariasVecesSeguidas_AcumulaLasEntradas()
        {
            var inventario = CrearInventario(cantidadActual: 0, cantidadMinima: 5);

            inventario.RegistrarEntradaStock(4);
            inventario.RegistrarEntradaStock(6);
            inventario.RegistrarEntradaStock(10);

            inventario.CantidadActual.Should().Be(20);
        }

        [Fact]
        public void RegistrarEntradaStock_NoTocaLaCantidadMinimaNiElProductoId()
        {
            var inventario = CrearInventario(cantidadActual: 10, cantidadMinima: 5, productoId: 42);

            inventario.RegistrarEntradaStock(5);

            inventario.CantidadMinima.Should().Be(5);
            inventario.ProductoId.Should().Be(42);
        }

        [Fact]
        public void RegistrarEntradaStock_QueSuperaElMinimo_DejaDeEstarEnStockBajo()
        {
            var inventario = CrearInventario(cantidadActual: 2, cantidadMinima: 5);
            inventario.EsStockBajo().Should().BeTrue("el escenario parte de un inventario bajo mínimo");

            inventario.RegistrarEntradaStock(10);

            inventario.EsStockBajo().Should().BeFalse();
        }

        // El delay hace determinista la comparación de timestamps: sin él, dos lecturas
        // consecutivas de DateTime.UtcNow pueden devolver el mismo valor.
        [Fact]
        public async Task RegistrarEntradaStock_ConCantidadValida_RefrescaUltimaActualizacionYModificadoEn()
        {
            var inventario = CrearInventario(cantidadActual: 10, cantidadMinima: 5);
            var ultimaActualizacionPrevia = inventario.UltimaActualizacion;
            var modificadoEnPrevio = inventario.ModificadoEn;
            await Task.Delay(10);

            inventario.RegistrarEntradaStock(5);

            inventario.UltimaActualizacion.Should().BeAfter(ultimaActualizacionPrevia);
            inventario.ModificadoEn.Should().BeAfter(modificadoEnPrevio);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void RegistrarEntradaStock_ConCantidadInvalida_NoModificaElStockNiLaFecha(int cantidad)
        {
            var inventario = CrearInventario(cantidadActual: 10, cantidadMinima: 5);
            var ultimaActualizacionPrevia = inventario.UltimaActualizacion;

            var accion = () => inventario.RegistrarEntradaStock(cantidad);

            accion.Should().Throw<ValidacionDominioException>()
                .WithMessage("*mayor a cero*")
                .Which.Campo.Should().Be("Cantidad");
            inventario.CantidadActual.Should().Be(10);
            inventario.UltimaActualizacion.Should().Be(ultimaActualizacionPrevia);
        }
    }
}
