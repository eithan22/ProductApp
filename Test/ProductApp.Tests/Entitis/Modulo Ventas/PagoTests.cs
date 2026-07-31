using FluentAssertions;
using ProductApp.Domian.Common.Enums.EnumsPago;
using ProductApp.Domian.Entitis;
using Xunit;

namespace ProductApp.Tests.Entitis
{
    public class PagoTests
    {
        private static Pago CrearPago()
            => new Pago(ordenId: 1, monto: 100, metodoPago: MetodoPago.Efectivo);

        [Fact]
        public void Constructor_DejaElPagoEnEstadoPendiente()
        {
            var pago = CrearPago();

            pago.Estado.Should().Be(EstadoPago.Pendiente);
        }

        [Fact]
        public void MarcarComoCompletado_LoDejaEnCompletado()
        {
            var pago = CrearPago();

            pago.MarcarComoCompletado();

            pago.Estado.Should().Be(EstadoPago.Completado);
        }
    }
}
