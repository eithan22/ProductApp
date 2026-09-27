using FluentAssertions;
using ProductApp.Aplication.Dtos.Modulo_Ventas.DetalleOrdenDto;
using ProductApp.Aplication.Dtos.OrdenDto;
using ProductApp.Aplication.Dtos.PagoDto;
using Xunit;

namespace ProductApp.Tests.Integration
{
    // M14 (auditoría de seguridad): antes, un pago de una orden cancelada seguía sumando en
    // Ingresos totales aunque Ventas por fecha ya no contara esa orden (misma deuda anotada en
    // A7). Y el ticket promedio se calculaba por pago, no por orden, así que dos abonos
    // parciales de la misma venta inflaban la cantidad de "ventas" del período.
    public class ReporteIngresosTotalesTests
    {
        [Fact]
        public async Task ObtenerIngresosTotalesAsync_ConPagoDeOrdenCancelada_NoLoCuentaComoIngreso()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var (_, producto, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 100, precio: 10);
            var cliente = await IntegrationTestFactory.SembrarClienteAsync(context);
            var usuario = await IntegrationTestFactory.SembrarUsuarioAsync(context);

            var ordenServices = IntegrationTestFactory.CrearOrdenServices(context);
            var detalleService = IntegrationTestFactory.CrearDetalleOrdenService(context);
            var pagoService = IntegrationTestFactory.CrearPagoService(context);

            // Orden A: se queda Pendiente con un pago parcial. Total = 60, abono = 30.
            var ordenAResult = await ordenServices.CrearOrden(new CreateOrdenDto { ClienteId = cliente.Id }, usuario.Id);
            var ordenAId = ordenAResult.Data!.Id;
            await detalleService.AgregarProductoAsync(new CreateDetalleOrdenDto { OrdenId = ordenAId, ProductId = producto.Id, Cantidad = 6 }, usuario.Id, esAdministrador: false);
            await pagoService.RegistrarPagoAsync(new CreatePagoDto { OrdenId = ordenAId, Monto = 30, MetodoPago = "Efectivo" }, usuario.Id, esAdministrador: false);

            // Orden B: recibe un abono de 30 y luego se cancela (requiere admin porque ya tiene pago).
            var ordenBResult = await ordenServices.CrearOrden(new CreateOrdenDto { ClienteId = cliente.Id }, usuario.Id);
            var ordenBId = ordenBResult.Data!.Id;
            await detalleService.AgregarProductoAsync(new CreateDetalleOrdenDto { OrdenId = ordenBId, ProductId = producto.Id, Cantidad = 6 }, usuario.Id, esAdministrador: false);
            await pagoService.RegistrarPagoAsync(new CreatePagoDto { OrdenId = ordenBId, Monto = 30, MetodoPago = "Efectivo" }, usuario.Id, esAdministrador: false);
            var cancelarResult = await ordenServices.CancelarOrden(ordenBId, usuario.Id, esAdministrador: true);
            cancelarResult.IsSuccess.Should().BeTrue(cancelarResult.Message);

            var reporteService = IntegrationTestFactory.CrearReporteService(context);
            var desde = DateTime.UtcNow.Date.AddDays(-1);
            var hasta = DateTime.UtcNow.Date.AddDays(1);

            var resultado = await reporteService.ObtenerIngresosTotalesAsync(desde, hasta);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.Total.Should().Be(30); // solo el abono de la orden A; excluye el de la cancelada
            resultado.Data.CantidadPagos.Should().Be(1);
        }

        [Fact]
        public async Task ObtenerIngresosTotalesAsync_ConDosAbonosDeLaMismaOrden_CalculaElTicketPromedioPorOrden()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var (_, producto, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 100, precio: 10);
            var cliente = await IntegrationTestFactory.SembrarClienteAsync(context);
            var usuario = await IntegrationTestFactory.SembrarUsuarioAsync(context);

            var ordenServices = IntegrationTestFactory.CrearOrdenServices(context);
            var detalleService = IntegrationTestFactory.CrearDetalleOrdenService(context);
            var pagoService = IntegrationTestFactory.CrearPagoService(context);

            // Una sola orden de 60, pagada en dos abonos de 30: sigue siendo UNA venta.
            var ordenResult = await ordenServices.CrearOrden(new CreateOrdenDto { ClienteId = cliente.Id }, usuario.Id);
            var ordenId = ordenResult.Data!.Id;
            await detalleService.AgregarProductoAsync(new CreateDetalleOrdenDto { OrdenId = ordenId, ProductId = producto.Id, Cantidad = 6 }, usuario.Id, esAdministrador: false);
            await pagoService.RegistrarPagoAsync(new CreatePagoDto { OrdenId = ordenId, Monto = 30, MetodoPago = "Efectivo" }, usuario.Id, esAdministrador: false);
            await pagoService.RegistrarPagoAsync(new CreatePagoDto { OrdenId = ordenId, Monto = 30, MetodoPago = "Efectivo" }, usuario.Id, esAdministrador: false);

            var reporteService = IntegrationTestFactory.CrearReporteService(context);
            var desde = DateTime.UtcNow.Date.AddDays(-1);
            var hasta = DateTime.UtcNow.Date.AddDays(1);

            var resultado = await reporteService.ObtenerIngresosTotalesAsync(desde, hasta);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.Total.Should().Be(60);
            resultado.Data.CantidadPagos.Should().Be(2);
            resultado.Data.TicketPromedio.Should().Be(60); // por orden, no 30 (que daría dividir entre los 2 pagos)
        }
    }
}
