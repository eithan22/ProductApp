using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductApp.Aplication.Dtos.Modulo_Ventas.DetalleOrdenDto;
using ProductApp.Aplication.Dtos.OrdenDto;
using ProductApp.Domian.Entitis;
using ProductApp.Infraesctructura.Persistencia.Contex;
using Xunit;

namespace ProductApp.Tests.Integration
{
    // Agregar, actualizar o eliminar renglones de una orden pendiente ajena no comparaba al
    // solicitante contra el dueño (mismo patrón que A1/A3, cerrado en esta misma auditoría).
    public class DetalleOrdenPropiedadTests
    {
        private const string MensajeBloqueo = "No tiene permiso sobre esta orden";

        private sealed record Escenario(int OrdenId, Usuario Dueno, Usuario Intruso, Producto ProductoA, Producto ProductoB, int DetalleAId);

        // Orden Pendiente del dueño con 3 unidades de A => Total 60.
        private static async Task<Escenario> SembrarOrdenConDetalleAsync(AppDbContext context)
        {
            var (_, productoA, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 10, precio: 20);
            var (_, productoB, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 50, precio: 5);
            var cliente = await IntegrationTestFactory.SembrarClienteAsync(context);
            var dueno = await IntegrationTestFactory.SembrarUsuarioAsync(context);
            var intruso = await IntegrationTestFactory.SembrarUsuarioAsync(
                context, nombre: "Vendedor Intruso", correo: "intruso@test.com", nombreUsuario: "vendedor.intruso");

            var ordenServices = IntegrationTestFactory.CrearOrdenServices(context);
            var detalleService = IntegrationTestFactory.CrearDetalleOrdenService(context);

            var crear = await ordenServices.CrearOrden(new CreateOrdenDto { ClienteId = cliente.Id }, dueno.Id);
            crear.IsSuccess.Should().BeTrue(crear.Message);

            var detalleA = await detalleService.AgregarProductoAsync(new CreateDetalleOrdenDto
            {
                OrdenId = crear.Data!.Id,
                ProductId = productoA.Id,
                Cantidad = 3
            }, dueno.Id, esAdministrador: false);
            detalleA.IsSuccess.Should().BeTrue(detalleA.Message);

            return new Escenario(crear.Data.Id, dueno, intruso, productoA, productoB, detalleA.Data!.ID);
        }

        private static async Task<decimal> TotalDeAsync(AppDbContext context, int ordenId)
            => (await context.Ordenes.FindAsync(ordenId))!.Total;

        [Fact]
        public async Task AgregarProductoAsync_ConOrdenDeOtroVendedor_DevuelveFailureYNoCreaElDetalle()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConDetalleAsync(context);
            var detalleService = IntegrationTestFactory.CrearDetalleOrdenService(context);

            var resultado = await detalleService.AgregarProductoAsync(new CreateDetalleOrdenDto
            {
                OrdenId = escenario.OrdenId,
                ProductId = escenario.ProductoB.Id,
                Cantidad = 4
            }, escenario.Intruso.Id, esAdministrador: false);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be(MensajeBloqueo);

            (await TotalDeAsync(context, escenario.OrdenId)).Should().Be(60m);
            (await context.DetalleOrden.CountAsync(d => d.OrdenId == escenario.OrdenId)).Should().Be(1);
        }

        [Fact]
        public async Task ActualizarDetalleOrden_ConOrdenDeOtroVendedor_DevuelveFailureYNoTocaElTotal()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConDetalleAsync(context);
            var detalleService = IntegrationTestFactory.CrearDetalleOrdenService(context);

            var resultado = await detalleService.ActualizarDetalleOrden(
                escenario.DetalleAId,
                new UpdateDetalleOrdenDto { id = escenario.DetalleAId, Cantidad = 1 },
                escenario.Intruso.Id,
                esAdministrador: false);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be(MensajeBloqueo);

            (await TotalDeAsync(context, escenario.OrdenId)).Should().Be(60m);
            (await context.DetalleOrden.FindAsync(escenario.DetalleAId))!.Cantidad.Should().Be(3);
        }

        [Fact]
        public async Task EliminarProductoAsync_ConOrdenDeOtroVendedor_DevuelveFailureYNoBorraElDetalle()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConDetalleAsync(context);
            var detalleService = IntegrationTestFactory.CrearDetalleOrdenService(context);

            var resultado = await detalleService.EliminarProductoAsync(escenario.DetalleAId, escenario.Intruso.Id, esAdministrador: false);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be(MensajeBloqueo);

            (await TotalDeAsync(context, escenario.OrdenId)).Should().Be(60m);
            (await context.DetalleOrden.FindAsync(escenario.DetalleAId)).Should().NotBeNull();
        }

        // Control de no regresión: el dueño sigue pudiendo editar su propia orden sin fricción.
        [Fact]
        public async Task AgregarActualizarYEliminar_ConElDuenoDeLaOrden_SiguenFuncionando()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConDetalleAsync(context);
            var detalleService = IntegrationTestFactory.CrearDetalleOrdenService(context);

            var agregar = await detalleService.AgregarProductoAsync(new CreateDetalleOrdenDto
            {
                OrdenId = escenario.OrdenId,
                ProductId = escenario.ProductoB.Id,
                Cantidad = 4
            }, escenario.Dueno.Id, esAdministrador: false);
            agregar.IsSuccess.Should().BeTrue(agregar.Message);
            // 3 * 20 + 4 * 5 = 80
            (await TotalDeAsync(context, escenario.OrdenId)).Should().Be(80m);

            var actualizar = await detalleService.ActualizarDetalleOrden(
                escenario.DetalleAId,
                new UpdateDetalleOrdenDto { id = escenario.DetalleAId, Cantidad = 2 },
                escenario.Dueno.Id,
                esAdministrador: false);
            actualizar.IsSuccess.Should().BeTrue(actualizar.Message);
            // 2 * 20 + 4 * 5 = 60
            (await TotalDeAsync(context, escenario.OrdenId)).Should().Be(60m);

            var eliminar = await detalleService.EliminarProductoAsync(agregar.Data!.ID, escenario.Dueno.Id, esAdministrador: false);
            eliminar.IsSuccess.Should().BeTrue(eliminar.Message);
            // Queda solo 2 * 20 = 40
            (await TotalDeAsync(context, escenario.OrdenId)).Should().Be(40m);
        }

        // Fija por escrito la decisión de política: un administrador sí puede operar sobre
        // el carrito pendiente de cualquier vendedor, igual que ya puede cancelar/cobrar.
        [Fact]
        public async Task EliminarProductoAsync_ComoAdministrador_SobreOrdenAjena_Funciona()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConDetalleAsync(context);
            var detalleService = IntegrationTestFactory.CrearDetalleOrdenService(context);

            var resultado = await detalleService.EliminarProductoAsync(escenario.DetalleAId, escenario.Intruso.Id, esAdministrador: true);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            (await TotalDeAsync(context, escenario.OrdenId)).Should().Be(0m);
            (await context.DetalleOrden.FindAsync(escenario.DetalleAId)).Should().BeNull();
        }
    }
}
