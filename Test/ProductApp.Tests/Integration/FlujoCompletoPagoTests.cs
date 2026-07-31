using FluentAssertions;
using ProductApp.Aplication.Dtos.Modulo_Ventas.DetalleOrdenDto;
using ProductApp.Aplication.Dtos.OrdenDto;
using ProductApp.Aplication.Dtos.PagoDto;
using ProductApp.Domian.Common.Enums.EnumsOrden;
using ProductApp.Domian.Common.Enums.EnumsPago;
using Xunit;

namespace ProductApp.Tests.Integration
{
    public class FlujoCompletoPagoTests
    {
        [Fact]
        public async Task FlujoCompleto_CrearOrden_AgregarProducto_Confirmar_Pagar_DejaOrdenPagadaYDescuentaStock()
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

            var confirmarResult = await ordenServices.ConfirmarOrden(ordenId, usuario.Id);
            confirmarResult.IsSuccess.Should().BeTrue(confirmarResult.Message);

            var pagoResult = await pagoService.RegistrarPagoAsync(new CreatePagoDto
            {
                OrdenId = ordenId,
                Monto = 60,
                MetodoPago = "Efectivo"
            }, usuario.Id);
            pagoResult.IsSuccess.Should().BeTrue(pagoResult.Message);

            var ordenFinal = await context.Ordenes.FindAsync(ordenId);
            ordenFinal!.Estado.Should().Be(EstadoOrden.Pagada);

            var inventarioFinal = await context.Inventario.FindAsync(inventario.Id);
            inventarioFinal!.CantidadActual.Should().Be(7);

            var pagos = context.Pagos.Where(p => p.OrdenId == ordenId).ToList();
            pagos.Should().ContainSingle(p => p.Estado == EstadoPago.Completado);
        }
    }
}
