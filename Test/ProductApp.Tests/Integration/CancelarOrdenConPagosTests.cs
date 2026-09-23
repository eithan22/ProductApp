using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductApp.Aplication.Dtos.Modulo_Ventas.DetalleOrdenDto;
using ProductApp.Aplication.Dtos.Modulo_Ventas.OrdenDto;
using ProductApp.Aplication.Dtos.OrdenDto;
using ProductApp.Aplication.Dtos.PagoDto;
using ProductApp.Domian.Common.Enums.EnumsNotificacion;
using ProductApp.Domian.Common.Enums.EnumsOrden;
using ProductApp.Domian.Entitis;
using ProductApp.Infraesctructura.Persistencia.Contex;
using Xunit;

namespace ProductApp.Tests.Integration
{
    // Un pago parcial deja la orden en Pendiente, así que la máquina de estados sí deja
    // cancelarla: el dinero cobrado quedaría sin contraparte. La política es que solo un
    // administrador puede hacerlo. Se cubren las dos puertas a la misma operación:
    // CancelarOrden y CambiarEstadoOrden con NuevoEstado = Cancelada.
    public class CancelarOrdenConPagosTests
    {
        private const string MensajeBloqueo =
            "Esta orden tiene pagos registrados; solicite a un administrador la cancelación.";

        private sealed record Escenario(int OrdenId, Usuario Usuario, Producto Producto, int InventarioId);

        // Orden Pendiente de 3 x 20 = 60.
        private static async Task<Escenario> SembrarOrdenConDetalleAsync(AppDbContext context)
        {
            var (_, producto, inventario) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 10, precio: 20);
            var cliente = await IntegrationTestFactory.SembrarClienteAsync(context);
            var usuario = await IntegrationTestFactory.SembrarUsuarioAsync(context);

            var ordenServices = IntegrationTestFactory.CrearOrdenServices(context);
            var detalleService = IntegrationTestFactory.CrearDetalleOrdenService(context);

            var crear = await ordenServices.CrearOrden(new CreateOrdenDto { ClienteId = cliente.Id }, usuario.Id);
            crear.IsSuccess.Should().BeTrue(crear.Message);

            var detalle = await detalleService.AgregarProductoAsync(new CreateDetalleOrdenDto
            {
                OrdenId = crear.Data!.Id,
                ProductId = producto.Id,
                Cantidad = 3
            }, usuario.Id, esAdministrador: false);
            detalle.IsSuccess.Should().BeTrue(detalle.Message);

            return new Escenario(crear.Data.Id, usuario, producto, inventario.Id);
        }

        // Paga 30 de 60: la orden sigue Pendiente y ya tiene un pago registrado.
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

        [Fact]
        public async Task CancelarOrden_ConPagoParcialSiendoVendedor_DevuelveFailureYNoTocaLaOrden()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConDetalleAsync(context);
            await RegistrarPagoParcialAsync(context, escenario);
            var ordenServices = IntegrationTestFactory.CrearOrdenServices(context);

            // Es el dueño de la orden: pasa el guard de propiedad y muere en el guard de pagos.
            var resultado = await ordenServices.CancelarOrden(escenario.OrdenId, escenario.Usuario.Id, esAdministrador: false);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be(MensajeBloqueo);

            (await context.Ordenes.FindAsync(escenario.OrdenId))!.Estado.Should().Be(EstadoOrden.Pendiente);
            // La notificación de la cancelación tampoco se dispara (RegistrarPagoParcialAsync ya
            // generó una de PagoRegistrado, así que se filtra por tipo en vez de contar todas).
            (await context.Notificaciones.CountAsync(n => n.Tipo == TipoNotificacion.CambioEstadoOrden)).Should().Be(0);
        }

        [Fact]
        public async Task CancelarOrden_ConPagoParcialSiendoAdministrador_CancelaLaOrden()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConDetalleAsync(context);
            await RegistrarPagoParcialAsync(context, escenario);
            var ordenServices = IntegrationTestFactory.CrearOrdenServices(context);

            var administrador = await IntegrationTestFactory.SembrarUsuarioAsync(
                context, "Usuario Admin", "admin@test.com", "usuario.admin");

            var resultado = await ordenServices.CancelarOrden(escenario.OrdenId, administrador.Id, esAdministrador: true);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            (await context.Ordenes.FindAsync(escenario.OrdenId))!.Estado.Should().Be(EstadoOrden.Cancelada);

            // El pago parcial no descuenta stock, y cancelar tampoco lo devuelve: sigue en 10.
            (await context.Inventario.FindAsync(escenario.InventarioId))!.CantidadActual.Should().Be(10);
            // La notificación de cancelación va al dueño de la orden, no al administrador que canceló.
            (await context.Notificaciones.SingleAsync(n => n.Tipo == TipoNotificacion.CambioEstadoOrden))
                .UsuarioId.Should().Be(escenario.Usuario.Id);
        }

        [Fact]
        public async Task CambiarEstadoOrden_ACanceladaConPagoParcialSiendoVendedor_DevuelveFailureYNoTocaLaOrden()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConDetalleAsync(context);
            await RegistrarPagoParcialAsync(context, escenario);
            var ordenServices = IntegrationTestFactory.CrearOrdenServices(context);

            // La segunda puerta al mismo bug: cambiar el estado a mano en vez de usar CancelarOrden.
            var resultado = await ordenServices.CambiarEstadoOrden(new CambiarEstadoOrdenDto
            {
                Id = escenario.OrdenId,
                NuevoEstado = nameof(EstadoOrden.Cancelada)
            }, escenario.Usuario.Id, esAdministrador: false);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be(MensajeBloqueo);

            (await context.Ordenes.FindAsync(escenario.OrdenId))!.Estado.Should().Be(EstadoOrden.Pendiente);
            (await context.Notificaciones.CountAsync(n => n.Tipo == TipoNotificacion.CambioEstadoOrden)).Should().Be(0);
        }

        [Fact]
        public async Task CambiarEstadoOrden_ACanceladaConPagoParcialSiendoAdministrador_PersisteElEstado()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConDetalleAsync(context);
            await RegistrarPagoParcialAsync(context, escenario);
            var ordenServices = IntegrationTestFactory.CrearOrdenServices(context);

            var resultado = await ordenServices.CambiarEstadoOrden(new CambiarEstadoOrdenDto
            {
                Id = escenario.OrdenId,
                NuevoEstado = nameof(EstadoOrden.Cancelada)
            }, escenario.Usuario.Id, esAdministrador: true);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            (await context.Ordenes.FindAsync(escenario.OrdenId))!.Estado.Should().Be(EstadoOrden.Cancelada);
        }

        // Control de no regresión: sin pagos, cancelar sigue siendo cosa del vendedor.
        [Fact]
        public async Task CancelarOrden_SinPagos_SigueFuncionandoParaVendedorYParaAdministrador()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenConDetalleAsync(context);
            var ordenServices = IntegrationTestFactory.CrearOrdenServices(context);

            var porVendedor = await ordenServices.CancelarOrden(escenario.OrdenId, escenario.Usuario.Id, esAdministrador: false);
            porVendedor.IsSuccess.Should().BeTrue(porVendedor.Message);
            (await context.Ordenes.FindAsync(escenario.OrdenId))!.Estado.Should().Be(EstadoOrden.Cancelada);

            // Segunda orden del mismo vendedor, cancelada por un administrador vía CambiarEstadoOrden.
            var otra = await ordenServices.CrearOrden(
                new CreateOrdenDto { ClienteId = (await context.Clientes.FirstAsync()).Id }, escenario.Usuario.Id);
            otra.IsSuccess.Should().BeTrue(otra.Message);

            var porAdministrador = await ordenServices.CambiarEstadoOrden(new CambiarEstadoOrdenDto
            {
                Id = otra.Data!.Id,
                NuevoEstado = nameof(EstadoOrden.Cancelada)
            }, escenario.Usuario.Id, esAdministrador: true);

            porAdministrador.IsSuccess.Should().BeTrue(porAdministrador.Message);
            (await context.Ordenes.FindAsync(otra.Data.Id))!.Estado.Should().Be(EstadoOrden.Cancelada);
        }
    }
}
