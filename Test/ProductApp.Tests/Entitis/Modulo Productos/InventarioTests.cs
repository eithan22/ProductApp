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
    }
}
