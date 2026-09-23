using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductApp.Aplication.Dtos.Modulo_Ventas.DetalleOrdenDto;
using ProductApp.Aplication.Dtos.Modulo_Ventas.OrdenDto;
using ProductApp.Aplication.Dtos.OrdenDto;
using ProductApp.Domian.Common.Enums.EnumsOrden;
using ProductApp.Domian.Entitis;
using ProductApp.Infraesctructura.Persistencia.Contex;
using Xunit;

namespace ProductApp.Tests.Integration
{
    // ActualizarDetalleOrden y EliminarProductoAsync recalculan Orden.Total; un error aquí
    // produce totales que después se cobran y se facturan.
    public class DetalleOrdenServiceRecalculoTests
    {
        private sealed record Escenario(int OrdenId, Usuario Usuario, Producto ProductoA, Producto ProductoB, int DetalleAId, int DetalleBId);

        // Dos productos con precios distintos (20 y 5) para que el total no pueda salir bien
        // "por casualidad" si el recálculo sumara cantidades en vez de subtotales.
        // Total inicial esperado: 2 * 20 + 4 * 5 = 60.
        private static async Task<Escenario> SembrarOrdenConDosProductosAsync(AppDbContext context)
        {
            var (_, productoA, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 20, precio: 20);
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
                Cantidad = 2
            }, usuario.Id, esAdministrador: false);
            detalleA.IsSuccess.Should().BeTrue(detalleA.Message);

            var detalleB = await detalleService.AgregarProductoAsync(new CreateDetalleOrdenDto
            {
                OrdenId = ordenId,
                ProductId = productoB.Id,
                Cantidad = 4
            }, usuario.Id, esAdministrador: false);
            detalleB.IsSuccess.Should().BeTrue(detalleB.Message);

            (await context.Ordenes.FindAsync(ordenId))!.Total.Should().Be(60m);

            return new Escenario(ordenId, usuario, productoA, productoB, detalleA.Data!.ID, detalleB.Data!.ID);
        }

        private static async Task<decimal> TotalDeAsync(AppDbContext context, int ordenId)
            => (await context.Ordenes.FindAsync(ordenId))!.Total;

        [Fact]
        public async Task ActualizarDetalleOrden_AlSubirLaCantidad_RecalculaElTotalDeLaOrden()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConDosProductosAsync(context);
            var detalleService = IntegrationTestFactory.CrearDetalleOrdenService(context);

            var resultado = await detalleService.ActualizarDetalleOrden(
                escenario.DetalleAId,
                new UpdateDetalleOrdenDto { id = escenario.DetalleAId, Cantidad = 5 },
                escenario.Usuario.Id,
                esAdministrador: false);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.Cantidad.Should().Be(5);
            resultado.Data.Subtotal.Should().Be(100m);

            // 5 * 20 + 4 * 5 = 120
            (await TotalDeAsync(context, escenario.OrdenId)).Should().Be(120m);
        }

        [Fact]
        public async Task ActualizarDetalleOrden_AlBajarLaCantidad_RecalculaElTotalHaciaAbajo()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConDosProductosAsync(context);
            var detalleService = IntegrationTestFactory.CrearDetalleOrdenService(context);

            var resultado = await detalleService.ActualizarDetalleOrden(
                escenario.DetalleBId,
                new UpdateDetalleOrdenDto { id = escenario.DetalleBId, Cantidad = 1 },
                escenario.Usuario.Id,
                esAdministrador: false);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);

            // 2 * 20 + 1 * 5 = 45
            (await TotalDeAsync(context, escenario.OrdenId)).Should().Be(45m);

            // El otro detalle queda intacto.
            (await context.DetalleOrden.FindAsync(escenario.DetalleAId))!.Subtotal.Should().Be(40m);
        }

        [Fact]
        public async Task ActualizarDetalleOrden_MantieneElPrecioUnitarioCongeladoAunqueCambieElPrecioDelProducto()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConDosProductosAsync(context);
            var detalleService = IntegrationTestFactory.CrearDetalleOrdenService(context);

            var productoA = await context.Productos.FindAsync(escenario.ProductoA.Id);
            productoA!.CambiarYvalidarPrecio(999m);
            await context.SaveChangesAsync();

            var resultado = await detalleService.ActualizarDetalleOrden(
                escenario.DetalleAId,
                new UpdateDetalleOrdenDto { id = escenario.DetalleAId, Cantidad = 3 },
                escenario.Usuario.Id,
                esAdministrador: false);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.PrecioUnitario.Should().Be(20m);

            // 3 * 20 + 4 * 5 = 80: el precio pactado al agregar el producto es el que manda.
            (await TotalDeAsync(context, escenario.OrdenId)).Should().Be(80m);
        }

        [Fact]
        public async Task EliminarProductoAsync_QuitaElDetalleYRecalculaElTotal()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConDosProductosAsync(context);
            var detalleService = IntegrationTestFactory.CrearDetalleOrdenService(context);

            var resultado = await detalleService.EliminarProductoAsync(escenario.DetalleBId, escenario.Usuario.Id, esAdministrador: false);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data.Should().BeTrue();

            // Queda solo 2 * 20 = 40
            (await TotalDeAsync(context, escenario.OrdenId)).Should().Be(40m);
            (await context.DetalleOrden.FindAsync(escenario.DetalleBId)).Should().BeNull();
            (await context.DetalleOrden.CountAsync(d => d.OrdenId == escenario.OrdenId)).Should().Be(1);
        }

        [Fact]
        public async Task EliminarProductoAsync_DelUltimoDetalle_DejaElTotalEnCeroYLaOrdenPendiente()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConDosProductosAsync(context);
            var detalleService = IntegrationTestFactory.CrearDetalleOrdenService(context);
            var ordenServices = IntegrationTestFactory.CrearOrdenServices(context);

            (await detalleService.EliminarProductoAsync(escenario.DetalleAId, escenario.Usuario.Id, esAdministrador: false)).IsSuccess.Should().BeTrue();
            (await detalleService.EliminarProductoAsync(escenario.DetalleBId, escenario.Usuario.Id, esAdministrador: false)).IsSuccess.Should().BeTrue();

            (await TotalDeAsync(context, escenario.OrdenId)).Should().Be(0m);
            (await context.Ordenes.FindAsync(escenario.OrdenId))!.Estado.Should().Be(EstadoOrden.Pendiente);

            // Una orden vaciada no se puede confirmar: el total en 0 es la barrera.
            var confirmar = await ordenServices.ConfirmarOrden(escenario.OrdenId, escenario.Usuario.Id, esAdministrador: true);
            confirmar.IsSuccess.Should().BeFalse();
            confirmar.Message.Should().Be("No se puede confirmar una orden sin productos");
        }

        [Fact]
        public async Task ActualizarDetalleOrden_SobreOrdenProcesada_DevuelveFailureYNoTocaElTotal()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConDosProductosAsync(context);
            var detalleService = IntegrationTestFactory.CrearDetalleOrdenService(context);
            var ordenServices = IntegrationTestFactory.CrearOrdenServices(context);

            (await ordenServices.ConfirmarOrden(escenario.OrdenId, escenario.Usuario.Id, esAdministrador: true))
                .IsSuccess.Should().BeTrue();

            var resultado = await detalleService.ActualizarDetalleOrden(
                escenario.DetalleAId,
                new UpdateDetalleOrdenDto { id = escenario.DetalleAId, Cantidad = 9 },
                escenario.Usuario.Id,
                esAdministrador: false);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be("No se pueden modificar detalles de una orden que no está pendiente");

            (await TotalDeAsync(context, escenario.OrdenId)).Should().Be(60m);
            (await context.DetalleOrden.FindAsync(escenario.DetalleAId))!.Cantidad.Should().Be(2);
        }

        [Fact]
        public async Task EliminarProductoAsync_SobreOrdenProcesada_DevuelveFailureYNoTocaElTotal()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConDosProductosAsync(context);
            var detalleService = IntegrationTestFactory.CrearDetalleOrdenService(context);
            var ordenServices = IntegrationTestFactory.CrearOrdenServices(context);

            (await ordenServices.ConfirmarOrden(escenario.OrdenId, escenario.Usuario.Id, esAdministrador: true))
                .IsSuccess.Should().BeTrue();

            var resultado = await detalleService.EliminarProductoAsync(escenario.DetalleBId, escenario.Usuario.Id, esAdministrador: false);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be("No se pueden eliminar productos de una orden que no está pendiente");

            (await TotalDeAsync(context, escenario.OrdenId)).Should().Be(60m);
            (await context.DetalleOrden.FindAsync(escenario.DetalleBId)).Should().NotBeNull();
        }

        [Fact]
        public async Task ActualizarDetalleOrden_SobreOrdenCancelada_DevuelveFailureYNoTocaElTotal()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConDosProductosAsync(context);
            var detalleService = IntegrationTestFactory.CrearDetalleOrdenService(context);
            var ordenServices = IntegrationTestFactory.CrearOrdenServices(context);

            (await ordenServices.CambiarEstadoOrden(new CambiarEstadoOrdenDto
            {
                Id = escenario.OrdenId,
                NuevoEstado = nameof(EstadoOrden.Cancelada)
            }, escenario.Usuario.Id, esAdministrador: true)).IsSuccess.Should().BeTrue();

            var resultado = await detalleService.ActualizarDetalleOrden(
                escenario.DetalleAId,
                new UpdateDetalleOrdenDto { id = escenario.DetalleAId, Cantidad = 1 },
                escenario.Usuario.Id,
                esAdministrador: false);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be("No se pueden modificar detalles de una orden que no está pendiente");
            (await TotalDeAsync(context, escenario.OrdenId)).Should().Be(60m);
        }

        [Fact]
        public async Task EliminarProductoAsync_SobreOrdenCancelada_DevuelveFailureYNoTocaElTotal()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConDosProductosAsync(context);
            var detalleService = IntegrationTestFactory.CrearDetalleOrdenService(context);
            var ordenServices = IntegrationTestFactory.CrearOrdenServices(context);

            (await ordenServices.CambiarEstadoOrden(new CambiarEstadoOrdenDto
            {
                Id = escenario.OrdenId,
                NuevoEstado = nameof(EstadoOrden.Cancelada)
            }, escenario.Usuario.Id, esAdministrador: true)).IsSuccess.Should().BeTrue();

            var resultado = await detalleService.EliminarProductoAsync(escenario.DetalleAId, escenario.Usuario.Id, esAdministrador: false);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be("No se pueden eliminar productos de una orden que no está pendiente");
            (await TotalDeAsync(context, escenario.OrdenId)).Should().Be(60m);
            (await context.DetalleOrden.CountAsync(d => d.OrdenId == escenario.OrdenId)).Should().Be(2);
        }

        [Fact]
        public async Task ActualizarDetalleOrden_ConCantidadQueExcedeElStock_DevuelveFailureYNoTocaElTotal()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConDosProductosAsync(context);
            var detalleService = IntegrationTestFactory.CrearDetalleOrdenService(context);

            // ProductoA tiene 20 unidades en inventario.
            var resultado = await detalleService.ActualizarDetalleOrden(
                escenario.DetalleAId,
                new UpdateDetalleOrdenDto { id = escenario.DetalleAId, Cantidad = 21 },
                escenario.Usuario.Id,
                esAdministrador: false);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be("Cantidad solicitada excede el stock disponible");
            (await TotalDeAsync(context, escenario.OrdenId)).Should().Be(60m);
        }

        [Fact]
        public async Task ActualizarDetalleOrden_ConCantidadCero_DevuelveFailureDelValidatorYNoTocaElTotal()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConDosProductosAsync(context);
            var detalleService = IntegrationTestFactory.CrearDetalleOrdenService(context);

            var resultado = await detalleService.ActualizarDetalleOrden(
                escenario.DetalleAId,
                new UpdateDetalleOrdenDto { id = escenario.DetalleAId, Cantidad = 0 },
                escenario.Usuario.Id,
                esAdministrador: false);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("La cantidad debe ser mayor que cero.");
            (await TotalDeAsync(context, escenario.OrdenId)).Should().Be(60m);
        }

        [Fact]
        public async Task ActualizarDetalleOrden_ConDetalleInexistente_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConDosProductosAsync(context);
            var detalleService = IntegrationTestFactory.CrearDetalleOrdenService(context);

            var resultado = await detalleService.ActualizarDetalleOrden(
                9999, new UpdateDetalleOrdenDto { id = 9999, Cantidad = 1 },
                escenario.Usuario.Id, esAdministrador: false);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be("Detalle de orden no encontrado");
            (await TotalDeAsync(context, escenario.OrdenId)).Should().Be(60m);
        }

        [Fact]
        public async Task EliminarProductoAsync_ConDetalleInexistente_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConDosProductosAsync(context);
            var detalleService = IntegrationTestFactory.CrearDetalleOrdenService(context);

            var resultado = await detalleService.EliminarProductoAsync(9999, escenario.Usuario.Id, esAdministrador: false);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be("Detalle de orden no encontrado");
            (await TotalDeAsync(context, escenario.OrdenId)).Should().Be(60m);
        }
    }
}
