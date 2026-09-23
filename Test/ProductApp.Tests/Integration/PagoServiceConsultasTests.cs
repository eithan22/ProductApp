using FluentAssertions;
using ProductApp.Aplication.Dtos.Modulo_Ventas.DetalleOrdenDto;
using ProductApp.Aplication.Dtos.OrdenDto;
using ProductApp.Aplication.Dtos.PagoDto;
using ProductApp.Aplication.Services;
using ProductApp.Domian.Common.Enums.EnumsOrden;
using ProductApp.Domian.Common.Enums.EnumsPago;
using ProductApp.Domian.Entitis;
using ProductApp.Infraesctructura.Persistencia.Contex;
using Xunit;

namespace ProductApp.Tests.Integration
{
    // ObtenerSaldoPendienteAsync es el número que decide si una orden queda saldada; hasta ahora
    // solo se ejercitaba de rebote dentro de RegistrarPagoAsync. Aquí se prueba de forma directa.
    public class PagoServiceConsultasTests
    {
        private sealed record Escenario(int OrdenId, Usuario Usuario, Cliente Cliente, Producto Producto);

        // Precio 20 y stock 100: el total de la orden queda en 20 * cantidad y el stock nunca
        // es el factor que hace fallar un pago completo.
        private static async Task<Escenario> SembrarOrdenConTotalAsync(AppDbContext context, int cantidad = 3)
        {
            var (_, producto, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 100, precio: 20);
            var cliente = await IntegrationTestFactory.SembrarClienteAsync(context);
            var usuario = await IntegrationTestFactory.SembrarUsuarioAsync(context);

            var ordenId = await CrearOrdenConProductoAsync(context, cliente.Id, usuario.Id, producto.Id, cantidad);

            return new Escenario(ordenId, usuario, cliente, producto);
        }

        private static async Task<int> CrearOrdenConProductoAsync(
            AppDbContext context, int clienteId, int usuarioId, int productoId, int cantidad)
        {
            var ordenServices = IntegrationTestFactory.CrearOrdenServices(context);
            var detalleService = IntegrationTestFactory.CrearDetalleOrdenService(context);

            var crear = await ordenServices.CrearOrden(new CreateOrdenDto { ClienteId = clienteId }, usuarioId);
            crear.IsSuccess.Should().BeTrue(crear.Message);

            var detalle = await detalleService.AgregarProductoAsync(new CreateDetalleOrdenDto
            {
                OrdenId = crear.Data!.Id,
                ProductId = productoId,
                Cantidad = cantidad
            });
            detalle.IsSuccess.Should().BeTrue(detalle.Message);

            return crear.Data.Id;
        }

        private static async Task PagarAsync(
            PagoService pagoService, int ordenId, decimal monto, int usuarioId, MetodoPago metodo = MetodoPago.Efectivo)
        {
            var resultado = await pagoService.RegistrarPagoAsync(new CreatePagoDto
            {
                OrdenId = ordenId,
                Monto = monto,
                MetodoPago = metodo.ToString()
            }, usuarioId);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
        }

        [Fact]
        public async Task ObtenerSaldoPendienteAsync_SinNingunPago_DevuelveElTotalCompleto()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConTotalAsync(context);
            var pagoService = IntegrationTestFactory.CrearPagoService(context);

            var resultado = await pagoService.ObtenerSaldoPendienteAsync(escenario.OrdenId);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data.Should().Be(60m);
            resultado.Message.Should().Be("Saldo pendiente obtenido exitosamente");
        }

        [Fact]
        public async Task ObtenerSaldoPendienteAsync_TrasUnPagoParcial_DevuelveTotalMenosLoPagado()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConTotalAsync(context);
            var pagoService = IntegrationTestFactory.CrearPagoService(context);

            await PagarAsync(pagoService, escenario.OrdenId, 20m, escenario.Usuario.Id);

            var resultado = await pagoService.ObtenerSaldoPendienteAsync(escenario.OrdenId);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data.Should().Be(40m);
            (await context.Ordenes.FindAsync(escenario.OrdenId))!.Estado.Should().Be(EstadoOrden.Pendiente);
        }

        [Fact]
        public async Task ObtenerSaldoPendienteAsync_TrasDosPagosParciales_AcumulaAmbosMontos()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConTotalAsync(context);
            var pagoService = IntegrationTestFactory.CrearPagoService(context);

            await PagarAsync(pagoService, escenario.OrdenId, 20m, escenario.Usuario.Id);
            await PagarAsync(pagoService, escenario.OrdenId, 15m, escenario.Usuario.Id, MetodoPago.TarjetaCredito);

            var resultado = await pagoService.ObtenerSaldoPendienteAsync(escenario.OrdenId);

            resultado.Data.Should().Be(25m);
        }

        [Fact]
        public async Task ObtenerSaldoPendienteAsync_ConLaOrdenYaPagadaCompletamente_DevuelveCero()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConTotalAsync(context);
            var pagoService = IntegrationTestFactory.CrearPagoService(context);

            await PagarAsync(pagoService, escenario.OrdenId, 20m, escenario.Usuario.Id);
            await PagarAsync(pagoService, escenario.OrdenId, 40m, escenario.Usuario.Id, MetodoPago.TransferenciaBancaria);

            var resultado = await pagoService.ObtenerSaldoPendienteAsync(escenario.OrdenId);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data.Should().Be(0m);
            (await context.Ordenes.FindAsync(escenario.OrdenId))!.Estado.Should().Be(EstadoOrden.Pagada);
        }

        [Fact]
        public async Task ObtenerSaldoPendienteAsync_NoCuentaLosPagosDeOtrasOrdenes()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConTotalAsync(context);
            var pagoService = IntegrationTestFactory.CrearPagoService(context);

            var ordenB = await CrearOrdenConProductoAsync(
                context, escenario.Cliente.Id, escenario.Usuario.Id, escenario.Producto.Id, cantidad: 1);

            await PagarAsync(pagoService, ordenB, 20m, escenario.Usuario.Id);

            (await pagoService.ObtenerSaldoPendienteAsync(escenario.OrdenId)).Data.Should().Be(60m);
            (await pagoService.ObtenerSaldoPendienteAsync(ordenB)).Data.Should().Be(0m);
        }

        [Fact]
        public async Task ObtenerSaldoPendienteAsync_ConOrdenInexistente_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            await SembrarOrdenConTotalAsync(context);
            var pagoService = IntegrationTestFactory.CrearPagoService(context);

            var resultado = await pagoService.ObtenerSaldoPendienteAsync(9999);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be("Orden no encontrada");
        }

        [Fact]
        public async Task ObtenerPagosPorOrdenAsync_SinPagos_DevuelveListaVaciaConMensajeExplicito()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConTotalAsync(context);
            var pagoService = IntegrationTestFactory.CrearPagoService(context);

            var resultado = await pagoService.ObtenerPagosPorOrdenAsync(escenario.OrdenId);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data.Should().BeEmpty();
            resultado.Message.Should().Be("Esta orden no tiene pagos todavía");
        }

        [Fact]
        public async Task ObtenerPagosPorOrdenAsync_DevuelveSoloLosPagosDeLaOrdenConsultada()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConTotalAsync(context);
            var pagoService = IntegrationTestFactory.CrearPagoService(context);

            var ordenB = await CrearOrdenConProductoAsync(
                context, escenario.Cliente.Id, escenario.Usuario.Id, escenario.Producto.Id, cantidad: 2);

            // El pago de la otra orden se registra primero a propósito: si el filtro por OrdenId
            // se cayera, aparecería mezclado en el resultado.
            await PagarAsync(pagoService, ordenB, 10m, escenario.Usuario.Id, MetodoPago.PayPal);
            await PagarAsync(pagoService, escenario.OrdenId, 20m, escenario.Usuario.Id);
            await PagarAsync(pagoService, escenario.OrdenId, 15m, escenario.Usuario.Id, MetodoPago.TarjetaDebito);

            var resultado = await pagoService.ObtenerPagosPorOrdenAsync(escenario.OrdenId);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            // El repositorio no aplica OrderBy, así que los pagos se comparan como conjunto.
            resultado.Data!.Select(p => p.Monto).Should().BeEquivalentTo(new[] { 20m, 15m });
            resultado.Data.Select(p => p.MetodoPago).Should().BeEquivalentTo(
                new[] { nameof(MetodoPago.Efectivo), nameof(MetodoPago.TarjetaDebito) });

            (await pagoService.ObtenerPagosPorOrdenAsync(ordenB)).Data!
                .Select(p => p.Monto).Should().BeEquivalentTo(new[] { 10m });
        }

        [Fact]
        public async Task ObtenerPagosPorOrdenAsync_MarcaComoCompletadoSoloElPagoQueSaldaLaOrden()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConTotalAsync(context);
            var pagoService = IntegrationTestFactory.CrearPagoService(context);

            await PagarAsync(pagoService, escenario.OrdenId, 20m, escenario.Usuario.Id);
            await PagarAsync(pagoService, escenario.OrdenId, 40m, escenario.Usuario.Id, MetodoPago.TransferenciaBancaria);

            var resultado = await pagoService.ObtenerPagosPorOrdenAsync(escenario.OrdenId);

            resultado.Data.Should().HaveCount(2);
            resultado.Data!.Should().ContainSingle(p => p.EstadoPago == nameof(EstadoPago.Completado))
                .Which.Monto.Should().Be(40m);
            resultado.Data.Should().ContainSingle(p => p.EstadoPago == nameof(EstadoPago.Pendiente))
                .Which.Monto.Should().Be(20m);
        }

        // HALLAZGO documentado: el DTO de cada pago no lleva el saldo que quedaba en ese momento,
        // sino el saldo actual de la orden repetido en todas las filas.
        [Fact]
        public async Task ObtenerPagosPorOrdenAsync_RepiteElSaldoActualEnTodosLosPagos()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConTotalAsync(context);
            var pagoService = IntegrationTestFactory.CrearPagoService(context);

            await PagarAsync(pagoService, escenario.OrdenId, 20m, escenario.Usuario.Id);
            await PagarAsync(pagoService, escenario.OrdenId, 15m, escenario.Usuario.Id, MetodoPago.TarjetaCredito);

            var resultado = await pagoService.ObtenerPagosPorOrdenAsync(escenario.OrdenId);

            resultado.Data!.Should().OnlyContain(p => p.SaldoPendiente == 25m);
        }

        [Fact]
        public async Task ObtenerPagosPorOrdenAsync_ConOrdenInexistente_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            await SembrarOrdenConTotalAsync(context);
            var pagoService = IntegrationTestFactory.CrearPagoService(context);

            var resultado = await pagoService.ObtenerPagosPorOrdenAsync(9999);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be("Orden no encontrada");
        }
    }
}
