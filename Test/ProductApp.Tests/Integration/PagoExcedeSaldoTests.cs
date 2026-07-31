using FluentAssertions;
using ProductApp.Aplication.Dtos.Modulo_Ventas.DetalleOrdenDto;
using ProductApp.Aplication.Dtos.OrdenDto;
using ProductApp.Aplication.Dtos.PagoDto;
using ProductApp.Domian.Common.Enums.EnumsOrden;
using Xunit;

namespace ProductApp.Tests.Integration
{
    public class PagoExcedeSaldoTests
    {
        [Fact]
        public async Task RegistrarPago_ConMontoMayorAlSaldo_FallaYNoModificaNada()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var (_, producto, inventario) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 10, precio: 20);
            var cliente = await IntegrationTestFactory.SembrarClienteAsync(context);
            var usuario = await IntegrationTestFactory.SembrarUsuarioAsync(context);

            var ordenServices = IntegrationTestFactory.CrearOrdenServices(context);
            var detalleService = IntegrationTestFactory.CrearDetalleOrdenService(context);
            var pagoService = IntegrationTestFactory.CrearPagoService(context);

            var crearResult = await ordenServices.CrearOrden(new CreateOrdenDto { ClienteId = cliente.Id }, usuario.Id);
            crearResult.IsSuccess.Should().BeTrue(crearResult.Message);
            var ordenId = crearResult.Data!.Id;

            var detalleResult = await detalleService.AgregarProductoAsync(new CreateDetalleOrdenDto
            {
                OrdenId = ordenId,
                ProductId = producto.Id,
                Cantidad = 3
            });
            detalleResult.IsSuccess.Should().BeTrue(detalleResult.Message);
            // Total = 60

            var pagoResult = await pagoService.RegistrarPagoAsync(new CreatePagoDto
            {
                OrdenId = ordenId,
                Monto = 100,
                MetodoPago = "Efectivo"
            }, usuario.Id);

            pagoResult.IsSuccess.Should().BeFalse();

            var ordenFinal = await context.Ordenes.FindAsync(ordenId);
            ordenFinal!.Estado.Should().Be(EstadoOrden.Pendiente);

            var inventarioFinal = await context.Inventario.FindAsync(inventario.Id);
            inventarioFinal!.CantidadActual.Should().Be(10);
        }
    }
}
