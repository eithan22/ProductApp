using FluentAssertions;
using ProductApp.Aplication.Dtos.Modulo_Ventas.DetalleOrdenDto;
using ProductApp.Aplication.Dtos.OrdenDto;
using Xunit;

namespace ProductApp.Tests.Integration
{
    public class ProductoInactivoNoVendibleTests
    {
        [Fact]
        public async Task AgregarProductoInactivo_FallaYNoCreaDetalleOrden()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var (_, producto, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 10, precio: 20);
            producto.DesactivarProducto();
            await context.SaveChangesAsync();

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
                Cantidad = 2
            });

            detalleResult.IsSuccess.Should().BeFalse();
            context.DetalleOrden.Count(d => d.OrdenId == ordenId).Should().Be(0);
        }
    }
}
