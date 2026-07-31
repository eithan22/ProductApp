using FluentAssertions;
using ProductApp.Domian.Common.Enums.EnumsOrden;
using ProductApp.Domian.Common.Exceptions;
using ProductApp.Domian.Entitis;
using Xunit;

namespace ProductApp.Tests.Entitis
{
    public class OrdenTests
    {
        private static Orden CrearOrden() => new Orden(clienteId: 1, usuarioId: 1);

        [Fact]
        public void Constructor_DejaLaOrdenEnEstadoPendiente()
        {
            var orden = CrearOrden();

            orden.Estado.Should().Be(EstadoOrden.Pendiente);
        }

        [Theory]
        [InlineData(EstadoOrden.Pendiente, EstadoOrden.Procesada)]
        [InlineData(EstadoOrden.Pendiente, EstadoOrden.Pagada)]
        [InlineData(EstadoOrden.Pendiente, EstadoOrden.Cancelada)]
        [InlineData(EstadoOrden.Procesada, EstadoOrden.Pagada)]
        [InlineData(EstadoOrden.Procesada, EstadoOrden.Cancelada)]
        [InlineData(EstadoOrden.Pagada, EstadoOrden.Entregada)]
        public void CambiarEstado_ConTransicionValida_CambiaElEstado(EstadoOrden estadoInicial, EstadoOrden estadoDestino)
        {
            var orden = CrearOrden();
            LlevarA(orden, estadoInicial);

            orden.CambiarEstado(estadoDestino);

            orden.Estado.Should().Be(estadoDestino);
        }

        [Theory]
        [InlineData(EstadoOrden.Cancelada, EstadoOrden.Pendiente)]
        [InlineData(EstadoOrden.Cancelada, EstadoOrden.Procesada)]
        [InlineData(EstadoOrden.Cancelada, EstadoOrden.Pagada)]
        [InlineData(EstadoOrden.Cancelada, EstadoOrden.Entregada)]
        [InlineData(EstadoOrden.Entregada, EstadoOrden.Pendiente)]
        [InlineData(EstadoOrden.Entregada, EstadoOrden.Procesada)]
        [InlineData(EstadoOrden.Entregada, EstadoOrden.Pagada)]
        [InlineData(EstadoOrden.Entregada, EstadoOrden.Cancelada)]
        [InlineData(EstadoOrden.Pagada, EstadoOrden.Cancelada)]
        [InlineData(EstadoOrden.Pagada, EstadoOrden.Procesada)]
        public void CambiarEstado_ConTransicionInvalida_LanzaEstadoInvalidoException(EstadoOrden estadoInicial, EstadoOrden estadoDestino)
        {
            var orden = CrearOrden();
            LlevarA(orden, estadoInicial);

            var accion = () => orden.CambiarEstado(estadoDestino);

            accion.Should().Throw<EstadoInvalidoException>();
        }

        [Fact]
        public void ConfirmarOrden_DejaLaOrdenEnProcesada()
        {
            var orden = CrearOrden();

            orden.ConfirmarOrden();

            orden.Estado.Should().Be(EstadoOrden.Procesada);
        }

        [Fact]
        public void CancelarOrden_DejaLaOrdenEnCancelada()
        {
            var orden = CrearOrden();

            orden.CancelarOrden();

            orden.Estado.Should().Be(EstadoOrden.Cancelada);
        }

        [Fact]
        public void ActualizarTotal_ConValorNegativo_LanzaValidacionDominioException()
        {
            var orden = CrearOrden();

            var accion = () => orden.ActualizarTotal(-10);

            accion.Should().Throw<ValidacionDominioException>();
        }

        [Fact]
        public void ActualizarTotal_ConValorValido_ActualizaElTotal()
        {
            var orden = CrearOrden();

            orden.ActualizarTotal(100);

            orden.Total.Should().Be(100);
        }

        [Fact]
        public void CalcularSaldoPendiente_DevuelveLaDiferenciaEntreTotalYPagado()
        {
            var orden = CrearOrden();
            orden.ActualizarTotal(100);

            orden.CalcularSaldoPendiente(80).Should().Be(20);
        }

        [Fact]
        public void EstaCompletamentePagada_ConTotalPagadoIgualAlTotal_DevuelveTrue()
        {
            var orden = CrearOrden();
            orden.ActualizarTotal(100);

            orden.EstaCompletamentePagada(100).Should().BeTrue();
        }

        [Fact]
        public void EstaCompletamentePagada_ConTotalPagadoMenorAlTotal_DevuelveFalse()
        {
            var orden = CrearOrden();
            orden.ActualizarTotal(100);

            orden.EstaCompletamentePagada(99).Should().BeFalse();
        }

        private static void LlevarA(Orden orden, EstadoOrden estado)
        {
            if (estado == EstadoOrden.Pendiente) return;

            switch (estado)
            {
                case EstadoOrden.Procesada:
                    orden.CambiarEstado(EstadoOrden.Procesada);
                    break;
                case EstadoOrden.Cancelada:
                    orden.CambiarEstado(EstadoOrden.Cancelada);
                    break;
                case EstadoOrden.Pagada:
                    orden.CambiarEstado(EstadoOrden.Pagada);
                    break;
                case EstadoOrden.Entregada:
                    orden.CambiarEstado(EstadoOrden.Pagada);
                    orden.CambiarEstado(EstadoOrden.Entregada);
                    break;
            }
        }
    }
}
