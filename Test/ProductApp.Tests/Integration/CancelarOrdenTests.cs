using FluentAssertions;
using ProductApp.Aplication.Dtos.Modulo_Ventas.DetalleOrdenDto;
using ProductApp.Aplication.Dtos.OrdenDto;
using ProductApp.Domian.Common.Enums.EnumsOrden;
using Xunit;

namespace ProductApp.Tests.Integration
{
    public class CancelarOrdenTests
    {
        [Fact]
        public async Task CancelarOrden_ConProductoAgregado_DejaOrdenCanceladaSinDescontarStock()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var (_, producto, inventario) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 10, precio: 20);
            var cliente = await IntegrationTestFactory.SembrarClienteAsync(context);
            var usuario = await IntegrationTestFactory.SembrarUsuarioAsync(context);

            var ordenServices = IntegrationTestFactory.CrearOrdenServices(context);
            var detalleService = IntegrationTestFactory.CrearDetalleOrdenService(context);

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

            var cancelarResult = await ordenServices.CancelarOrden(ordenId, usuario.Id);
            cancelarResult.IsSuccess.Should().BeTrue(cancelarResult.Message);

            var ordenFinal = await context.Ordenes.FindAsync(ordenId);
            ordenFinal!.Estado.Should().Be(EstadoOrden.Cancelada);

            var inventarioFinal = await context.Inventario.FindAsync(inventario.Id);
            inventarioFinal!.CantidadActual.Should().Be(10);
        }
    }
}
