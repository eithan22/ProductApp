using FluentAssertions;
using ProductApp.Aplication.Dtos.Modulo_Ventas.DetalleOrdenDto;
using ProductApp.Aplication.Dtos.OrdenDto;
using ProductApp.Aplication.Dtos.PagoDto;
using Xunit;

namespace ProductApp.Tests.Integration
{
    public class ReporteVentasPorFechaExcluyeCanceladaTests
    {
        [Fact]
        public async Task ObtenerVentasPorFechaAsync_SumaSoloOrdenesNoCanceladas()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var (_, producto, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 100, precio: 10);
            var cliente = await IntegrationTestFactory.SembrarClienteAsync(context);
            var usuario = await IntegrationTestFactory.SembrarUsuarioAsync(context);

            var ordenServices = IntegrationTestFactory.CrearOrdenServices(context);
            var detalleService = IntegrationTestFactory.CrearDetalleOrdenService(context);
            var pagoService = IntegrationTestFactory.CrearPagoService(context);

            // Orden A: termina Pagada. Total = 3 * 10 = 30.
            var ordenAResult = await ordenServices.CrearOrden(new CreateOrdenDto { ClienteId = cliente.Id }, usuario.Id);
            ordenAResult.IsSuccess.Should().BeTrue(ordenAResult.Message);
            var ordenAId = ordenAResult.Data!.Id;
            var detalleAResult = await detalleService.AgregarProductoAsync(new CreateDetalleOrdenDto { OrdenId = ordenAId, ProductId = producto.Id, Cantidad = 3 });
            detalleAResult.IsSuccess.Should().BeTrue(detalleAResult.Message);
            var pagoAResult = await pagoService.RegistrarPagoAsync(new CreatePagoDto { OrdenId = ordenAId, Monto = 30, MetodoPago = "Efectivo" }, usuario.Id);
            pagoAResult.IsSuccess.Should().BeTrue(pagoAResult.Message);

            // Orden B: termina Cancelada. Total = 5 * 10 = 50 (no debe contar).
            var ordenBResult = await ordenServices.CrearOrden(new CreateOrdenDto { ClienteId = cliente.Id }, usuario.Id);
            ordenBResult.IsSuccess.Should().BeTrue(ordenBResult.Message);
            var ordenBId = ordenBResult.Data!.Id;
            var detalleBResult = await detalleService.AgregarProductoAsync(new CreateDetalleOrdenDto { OrdenId = ordenBId, ProductId = producto.Id, Cantidad = 5 });
            detalleBResult.IsSuccess.Should().BeTrue(detalleBResult.Message);
            var cancelarBResult = await ordenServices.CancelarOrden(ordenBId, usuario.Id);
            cancelarBResult.IsSuccess.Should().BeTrue(cancelarBResult.Message);

            // Orden C: se queda Pendiente. Total = 4 * 10 = 40.
            var ordenCResult = await ordenServices.CrearOrden(new CreateOrdenDto { ClienteId = cliente.Id }, usuario.Id);
            ordenCResult.IsSuccess.Should().BeTrue(ordenCResult.Message);
            var ordenCId = ordenCResult.Data!.Id;
            var detalleCResult = await detalleService.AgregarProductoAsync(new CreateDetalleOrdenDto { OrdenId = ordenCId, ProductId = producto.Id, Cantidad = 4 });
            detalleCResult.IsSuccess.Should().BeTrue(detalleCResult.Message);

            var reporteService = IntegrationTestFactory.CrearReporteService(context);
            var desde = DateTime.UtcNow.Date.AddDays(-1);
            var hasta = DateTime.UtcNow.Date.AddDays(1);

            var resultado = await reporteService.ObtenerVentasPorFechaAsync(desde, hasta);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            var totalReportado = resultado.Data!.Sum(v => v.Total);
            totalReportado.Should().Be(70); // 30 (Pagada) + 40 (Pendiente); excluye los 50 de la Cancelada
        }
    }
}
