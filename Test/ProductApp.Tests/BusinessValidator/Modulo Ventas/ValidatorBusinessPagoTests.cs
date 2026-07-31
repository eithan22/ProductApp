using FluentAssertions;
using ProductApp.Aplication.BusinessValidator.Modulo_Ventas;
using ProductApp.Aplication.Dtos.PagoDto;
using ProductApp.Domian.Common.Enums.EnumsOrden;
using ProductApp.Domian.Entitis;
using Xunit;

namespace ProductApp.Tests.BusinessValidator.Modulo_Ventas
{
    public class ValidatorBusinessPagoTests
    {
        private readonly ValidatorBusinessPago _validator = new();

        private static Orden CrearOrden() => new Orden(clienteId: 1, usuarioId: 1);

        private static CreatePagoDto CrearDto(decimal monto)
            => new() { OrdenId = 1, Monto = monto, MetodoPago = "Efectivo" };

        [Fact]
        public async Task ValidarRegistrarPagoAsync_OrdenCancelada_DevuelveFailure()
        {
            var orden = CrearOrden();
            orden.ActualizarTotal(100);
            orden.CambiarEstado(EstadoOrden.Cancelada);

            var resultado = await _validator.ValidarRegistrarPagoAsync(CrearDto(50), orden, saldoActual: 100);

            resultado.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task ValidarRegistrarPagoAsync_OrdenPagada_DevuelveFailure()
        {
            var orden = CrearOrden();
            orden.ActualizarTotal(100);
            orden.CambiarEstado(EstadoOrden.Pagada);

            var resultado = await _validator.ValidarRegistrarPagoAsync(CrearDto(50), orden, saldoActual: 0);

            resultado.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task ValidarRegistrarPagoAsync_OrdenEntregada_DevuelveFailure()
        {
            var orden = CrearOrden();
            orden.ActualizarTotal(100);
            orden.CambiarEstado(EstadoOrden.Pagada);
            orden.CambiarEstado(EstadoOrden.Entregada);

            var resultado = await _validator.ValidarRegistrarPagoAsync(CrearDto(50), orden, saldoActual: 0);

            resultado.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task ValidarRegistrarPagoAsync_OrdenPendienteConTotalYMontoValido_DevuelveSuccess()
        {
            var orden = CrearOrden();
            orden.ActualizarTotal(100);

            var resultado = await _validator.ValidarRegistrarPagoAsync(CrearDto(60), orden, saldoActual: 100);

            resultado.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task ValidarRegistrarPagoAsync_OrdenProcesadaConTotalYMontoValido_DevuelveSuccess()
        {
            var orden = CrearOrden();
            orden.ConfirmarOrden();
            orden.ActualizarTotal(100);

            var resultado = await _validator.ValidarRegistrarPagoAsync(CrearDto(60), orden, saldoActual: 100);

            resultado.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task ValidarRegistrarPagoAsync_OrdenConTotalEnCero_DevuelveFailure()
        {
            var orden = CrearOrden();

            var resultado = await _validator.ValidarRegistrarPagoAsync(CrearDto(10), orden, saldoActual: 100);

            resultado.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task ValidarRegistrarPagoAsync_MontoMayorAlSaldo_DevuelveFailureConMontoYSaldoEnElMensaje()
        {
            var orden = CrearOrden();
            orden.ActualizarTotal(100);

            var resultado = await _validator.ValidarRegistrarPagoAsync(CrearDto(150), orden, saldoActual: 100);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("150");
            resultado.Message.Should().Contain("100");
        }
    }
}
