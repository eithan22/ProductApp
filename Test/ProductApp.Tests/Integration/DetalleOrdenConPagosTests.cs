using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductApp.Aplication.Dtos.Modulo_Ventas.DetalleOrdenDto;
using ProductApp.Aplication.Dtos.OrdenDto;
using ProductApp.Aplication.Dtos.PagoDto;
using ProductApp.Domian.Common.Enums.EnumsOrden;
using ProductApp.Domian.Entitis;
using ProductApp.Infraesctructura.Persistencia.Contex;
using Xunit;

namespace ProductApp.Tests.Integration
{
    // Un pago parcial deja la orden en Pendiente, así que el guard de estado no la protege:
    // sin un guard por pagos, editar renglones aquí mueve Orden.Total y desincroniza el saldo
    // ya cobrado (puede quedar pagado > total, es decir saldo negativo).
    public class DetalleOrdenConPagosTests
    {
        private sealed record Escenario(int OrdenId, Usuario Usuario, Producto ProductoA, Producto ProductoB, int DetalleAId);

        // ProductoA: precio 20, stock 10. ProductoB: precio 5, stock 50 (sirve para el caso "agregar").
        // Orden con 3 unidades de A => Total 60.
        private static async Task<Escenario> SembrarOrdenConDetalleAsync(AppDbContext context)
        {
            var (_, productoA, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 10, precio: 20);
            var (_, productoB, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 50, precio: 5);
            var cliente = await IntegrationTestFactory.SembrarClienteAsync(context);
            var usuario = await IntegrationTestFactory.SembrarUsuarioAsync(context);

            var ordenServices = IntegrationTestFactory.CrearOrdenServices(context);
            var detalleService = IntegrationTestFactory.CrearDetalleOrdenService(context);

            var crear = await ordenServices.CrearOrden(new CreateOrdenDto { ClienteId = cliente.Id }, usuario.Id);
            crear.IsSuccess.Should().BeTrue(crear.Message);
            var ordenId = crear.Data!.Id;

            var detalleA = await detalleService.AgregarProductoAsync(new CreateDetalleOrdenDto
            {
                OrdenId = ordenId,
                ProductId = productoA.Id,
                Cantidad = 3
            });
            detalleA.IsSuccess.Should().BeTrue(detalleA.Message);

            (await context.Ordenes.FindAsync(ordenId))!.Total.Should().Be(60m);

            return new Escenario(ordenId, usuario, productoA, productoB, detalleA.Data!.ID);
        }

        // Paga 30 de 60: la orden queda Pendiente y con un pago registrado, que es
        // exactamente el estado donde el guard de estado no alcanza.
        private static async Task RegistrarPagoParcialAsync(AppDbContext context, Escenario escenario)
        {
            var pagoService = IntegrationTestFactory.CrearPagoService(context);

            var pago = await pagoService.RegistrarPagoAsync(new CreatePagoDto
            {
                OrdenId = escenario.OrdenId,
                Monto = 30,
                MetodoPago = "Efectivo"
            }, escenario.Usuario.Id, esAdministrador: false);

            pago.IsSuccess.Should().BeTrue(pago.Message);
            (await context.Ordenes.FindAsync(escenario.OrdenId))!.Estado.Should().Be(EstadoOrden.Pendiente);
        }

        private static async Task<decimal> TotalDeAsync(AppDbContext context, int ordenId)
            => (await context.Ordenes.FindAsync(ordenId))!.Total;

        [Fact]
        public async Task AgregarProductoAsync_ConPagoRegistrado_DevuelveFailureYNoTocaElTotal()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConDetalleAsync(context);
            await RegistrarPagoParcialAsync(context, escenario);
            var detalleService = IntegrationTestFactory.CrearDetalleOrdenService(context);

            var resultado = await detalleService.AgregarProductoAsync(new CreateDetalleOrdenDto
            {
                OrdenId = escenario.OrdenId,
                ProductId = escenario.ProductoB.Id,
                Cantidad = 4
            });

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be("No se pueden agregar productos a una orden que ya tiene pagos registrados");

            (await TotalDeAsync(context, escenario.OrdenId)).Should().Be(60m);
            (await context.DetalleOrden.CountAsync(d => d.OrdenId == escenario.OrdenId)).Should().Be(1);
        }

        [Fact]
        public async Task ActualizarDetalleOrden_ConPagoRegistrado_DevuelveFailureYNoTocaElTotal()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConDetalleAsync(context);
            await RegistrarPagoParcialAsync(context, escenario);
            var detalleService = IntegrationTestFactory.CrearDetalleOrdenService(context);

            // Bajar a 1 unidad dejaría Total = 20 con 30 ya cobrados: saldo negativo.
            var resultado = await detalleService.ActualizarDetalleOrden(
                escenario.DetalleAId,
                new UpdateDetalleOrdenDto { id = escenario.DetalleAId, Cantidad = 1 });

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be("No se pueden modificar detalles de una orden que ya tiene pagos registrados");

            (await TotalDeAsync(context, escenario.OrdenId)).Should().Be(60m);
            (await context.DetalleOrden.FindAsync(escenario.DetalleAId))!.Cantidad.Should().Be(3);
        }

        [Fact]
        public async Task EliminarProductoAsync_ConPagoRegistrado_DevuelveFailureYNoTocaElTotal()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConDetalleAsync(context);
            await RegistrarPagoParcialAsync(context, escenario);
            var detalleService = IntegrationTestFactory.CrearDetalleOrdenService(context);

            // Vaciar la orden dejaría Total = 0 con 30 ya cobrados.
            var resultado = await detalleService.EliminarProductoAsync(escenario.DetalleAId);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be("No se pueden eliminar productos de una orden que ya tiene pagos registrados");

            (await TotalDeAsync(context, escenario.OrdenId)).Should().Be(60m);
            (await context.DetalleOrden.FindAsync(escenario.DetalleAId)).Should().NotBeNull();
        }

        // Control de no regresión: sin pagos, el flujo de edición sigue funcionando igual.
        [Fact]
        public async Task AgregarActualizarYEliminar_SinPagos_SiguenFuncionandoYRecalculanElTotal()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConDetalleAsync(context);
            var detalleService = IntegrationTestFactory.CrearDetalleOrdenService(context);

            var agregar = await detalleService.AgregarProductoAsync(new CreateDetalleOrdenDto
            {
                OrdenId = escenario.OrdenId,
                ProductId = escenario.ProductoB.Id,
                Cantidad = 4
            });
            agregar.IsSuccess.Should().BeTrue(agregar.Message);
            // 3 * 20 + 4 * 5 = 80
            (await TotalDeAsync(context, escenario.OrdenId)).Should().Be(80m);

            var actualizar = await detalleService.ActualizarDetalleOrden(
                escenario.DetalleAId,
                new UpdateDetalleOrdenDto { id = escenario.DetalleAId, Cantidad = 2 });
            actualizar.IsSuccess.Should().BeTrue(actualizar.Message);
            // 2 * 20 + 4 * 5 = 60
            (await TotalDeAsync(context, escenario.OrdenId)).Should().Be(60m);

            var eliminar = await detalleService.EliminarProductoAsync(agregar.Data!.ID);
            eliminar.IsSuccess.Should().BeTrue(eliminar.Message);
            // Queda solo 2 * 20 = 40
            (await TotalDeAsync(context, escenario.OrdenId)).Should().Be(40m);
            (await context.DetalleOrden.CountAsync(d => d.OrdenId == escenario.OrdenId)).Should().Be(1);
        }
    }
}
