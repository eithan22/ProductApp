using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductApp.Aplication.Dtos.Modulo_Ventas.DetalleOrdenDto;
using ProductApp.Aplication.Dtos.Modulo_Ventas.OrdenDto;
using ProductApp.Aplication.Dtos.OrdenDto;
using ProductApp.Aplication.Dtos.PagoDto;
using ProductApp.Domian.Common.Enums.EnumsNotificacion;
using ProductApp.Domian.Common.Enums.EnumsOrden;
using ProductApp.Domian.Common.Enums.EnumsPago;
using ProductApp.Domian.Common.Exceptions;
using ProductApp.Domian.Entitis;
using ProductApp.Infraesctructura.Persistencia.Contex;
using Xunit;

namespace ProductApp.Tests.Integration
{
    // La máquina de estados ya está probada en OrdenTests (entidad). Lo que se prueba aquí es la
    // capa de servicio: qué transiciones deja pasar el validator de negocio, qué se persiste y
    // qué notificación se dispara.
    public class OrdenServiceCambiarEstadoTests
    {
        private sealed record Escenario(int OrdenId, Usuario Usuario, Producto Producto);

        // Orden Pendiente con un detalle de 2 x 20 = 40, para que también se pueda pagar entera.
        private static async Task<Escenario> SembrarOrdenPendienteAsync(AppDbContext context)
        {
            var (_, producto, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 10, precio: 20);
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
                Cantidad = 2
            });
            detalle.IsSuccess.Should().BeTrue(detalle.Message);

            return new Escenario(crear.Data.Id, usuario, producto);
        }

        [Fact]
        public async Task CambiarEstadoOrden_DePendienteACancelada_PersisteElEstadoYNotificaAlDuenoDeLaOrden()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenPendienteAsync(context);
            var ordenServices = IntegrationTestFactory.CrearOrdenServices(context);

            var resultado = await ordenServices.CambiarEstadoOrden(new CambiarEstadoOrdenDto
            {
                Id = escenario.OrdenId,
                NuevoEstado = nameof(EstadoOrden.Cancelada)
            }, escenario.Usuario.Id, esAdministrador: true);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data.Should().BeTrue();
            resultado.Message.Should().Be("Estado de la orden actualizado exitosamente");

            (await context.Ordenes.FindAsync(escenario.OrdenId))!.Estado.Should().Be(EstadoOrden.Cancelada);

            var notificacion = await context.Notificaciones.SingleAsync();
            notificacion.UsuarioId.Should().Be(escenario.Usuario.Id);
            notificacion.Tipo.Should().Be(TipoNotificacion.CambioEstadoOrden);
            notificacion.Mensaje.Should().Be($"Tu orden #{escenario.OrdenId} pasó a estado Cancelada.");
        }

        [Fact]
        public async Task CambiarEstadoOrden_DeProcesadaACancelada_PersisteElEstado()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenPendienteAsync(context);
            var ordenServices = IntegrationTestFactory.CrearOrdenServices(context);

            (await ordenServices.ConfirmarOrden(escenario.OrdenId, escenario.Usuario.Id, esAdministrador: true))
                .IsSuccess.Should().BeTrue();

            var resultado = await ordenServices.CambiarEstadoOrden(new CambiarEstadoOrdenDto
            {
                Id = escenario.OrdenId,
                NuevoEstado = nameof(EstadoOrden.Cancelada)
            }, escenario.Usuario.Id, esAdministrador: true);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            (await context.Ordenes.FindAsync(escenario.OrdenId))!.Estado.Should().Be(EstadoOrden.Cancelada);

            // Una notificación por ConfirmarOrden y otra por CambiarEstadoOrden: cada transición
            // avisa exactamente una vez.
            (await context.Notificaciones.CountAsync(n => n.Tipo == TipoNotificacion.CambioEstadoOrden)).Should().Be(2);
        }

        [Fact]
        public async Task CambiarEstadoOrden_DePagadaAEntregada_PersisteElEstadoYNoVuelveADescontarStock()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenPendienteAsync(context);
            var ordenServices = IntegrationTestFactory.CrearOrdenServices(context);
            var pagoService = IntegrationTestFactory.CrearPagoService(context);

            // Entregada solo es alcanzable desde Pagada, y Pagada no se puede asignar a mano:
            // hay que pasar por el pago completo (total = 2 * 20).
            var pago = await pagoService.RegistrarPagoAsync(new CreatePagoDto
            {
                OrdenId = escenario.OrdenId,
                Monto = 40m,
                MetodoPago = nameof(MetodoPago.Efectivo)
            }, escenario.Usuario.Id, esAdministrador: false);
            pago.IsSuccess.Should().BeTrue(pago.Message);

            var resultado = await ordenServices.CambiarEstadoOrden(new CambiarEstadoOrdenDto
            {
                Id = escenario.OrdenId,
                NuevoEstado = nameof(EstadoOrden.Entregada)
            }, escenario.Usuario.Id, esAdministrador: true);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            (await context.Ordenes.FindAsync(escenario.OrdenId))!.Estado.Should().Be(EstadoOrden.Entregada);

            // El stock ya se descontó al pagar (10 - 2): entregar no lo vuelve a tocar.
            (await context.Inventario.SingleAsync(i => i.ProductoId == escenario.Producto.Id))
                .CantidadActual.Should().Be(8);

            (await context.Notificaciones.SingleAsync(n => n.Tipo == TipoNotificacion.CambioEstadoOrden))
                .Mensaje.Should().Be($"Tu orden #{escenario.OrdenId} pasó a estado Entregada.");
        }

        [Fact]
        public async Task CambiarEstadoOrden_AceptaElNombreDelEstadoSinDistinguirMayusculas()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenPendienteAsync(context);
            var ordenServices = IntegrationTestFactory.CrearOrdenServices(context);

            var resultado = await ordenServices.CambiarEstadoOrden(new CambiarEstadoOrdenDto
            {
                Id = escenario.OrdenId,
                NuevoEstado = "cancelada"
            }, escenario.Usuario.Id, esAdministrador: true);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            (await context.Ordenes.FindAsync(escenario.OrdenId))!.Estado.Should().Be(EstadoOrden.Cancelada);
        }

        // HALLAZGO: el servicio NO traduce EstadoInvalidoException a OperationResult.Failure.
        // La excepción de dominio sale del servicio y solo la atrapa GlobalExceptionHandler
        // (400 con el mensaje real). El test documenta el comportamiento actual.
        [Fact]
        public async Task CambiarEstadoOrden_ConTransicionInvalida_PropagaEstadoInvalidoExceptionYNoTocaLaOrden()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenPendienteAsync(context);
            var ordenServices = IntegrationTestFactory.CrearOrdenServices(context);

            // Entregada pasa el validator de negocio (solo bloquea Pagada y Procesada) pero no es
            // alcanzable desde Pendiente según el diccionario de transiciones de la entidad.
            var accion = async () => await ordenServices.CambiarEstadoOrden(new CambiarEstadoOrdenDto
            {
                Id = escenario.OrdenId,
                NuevoEstado = nameof(EstadoOrden.Entregada)
            }, escenario.Usuario.Id, esAdministrador: true);

            (await accion.Should().ThrowAsync<EstadoInvalidoException>())
                .WithMessage("No se puede ejecutar 'cambiar a Entregada' en 'Orden' con estado 'Pendiente'.");

            (await context.Ordenes.FindAsync(escenario.OrdenId))!.Estado.Should().Be(EstadoOrden.Pendiente);
            (await context.Notificaciones.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task CambiarEstadoOrden_SobreUnaOrdenYaCancelada_PropagaEstadoInvalidoException()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenPendienteAsync(context);
            var ordenServices = IntegrationTestFactory.CrearOrdenServices(context);

            (await ordenServices.CambiarEstadoOrden(new CambiarEstadoOrdenDto
            {
                Id = escenario.OrdenId,
                NuevoEstado = nameof(EstadoOrden.Cancelada)
            }, escenario.Usuario.Id, esAdministrador: true)).IsSuccess.Should().BeTrue();

            var accion = async () => await ordenServices.CambiarEstadoOrden(new CambiarEstadoOrdenDto
            {
                Id = escenario.OrdenId,
                NuevoEstado = nameof(EstadoOrden.Cancelada)
            }, escenario.Usuario.Id, esAdministrador: true);

            (await accion.Should().ThrowAsync<EstadoInvalidoException>())
                .WithMessage("No se puede ejecutar 'cambiar a Cancelada' en 'Orden' con estado 'Cancelada'.");

            (await context.Ordenes.FindAsync(escenario.OrdenId))!.Estado.Should().Be(EstadoOrden.Cancelada);
            // La segunda llamada no llega a notificar: la excepción corta antes.
            (await context.Notificaciones.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task CambiarEstadoOrden_ConEstadoPagada_DevuelveFailureSinNotificar()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenPendienteAsync(context);
            var ordenServices = IntegrationTestFactory.CrearOrdenServices(context);

            var resultado = await ordenServices.CambiarEstadoOrden(new CambiarEstadoOrdenDto
            {
                Id = escenario.OrdenId,
                NuevoEstado = nameof(EstadoOrden.Pagada)
            }, escenario.Usuario.Id, esAdministrador: true);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be(
                "El estado 'Pagada' no se puede asignar manualmente; se establece automáticamente al registrar un pago completo.");

            (await context.Ordenes.FindAsync(escenario.OrdenId))!.Estado.Should().Be(EstadoOrden.Pendiente);
            (await context.Notificaciones.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task CambiarEstadoOrden_ConEstadoProcesada_DevuelveFailureSinNotificar()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenPendienteAsync(context);
            var ordenServices = IntegrationTestFactory.CrearOrdenServices(context);

            var resultado = await ordenServices.CambiarEstadoOrden(new CambiarEstadoOrdenDto
            {
                Id = escenario.OrdenId,
                NuevoEstado = nameof(EstadoOrden.Procesada)
            }, escenario.Usuario.Id, esAdministrador: true);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be(
                "El estado 'Procesada' no se puede asignar manualmente; use el endpoint ConfirmarOrden.");

            (await context.Ordenes.FindAsync(escenario.OrdenId))!.Estado.Should().Be(EstadoOrden.Pendiente);
            (await context.Notificaciones.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task CambiarEstadoOrden_ConOrdenInexistente_DevuelveFailureSinNotificar()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenPendienteAsync(context);
            var ordenServices = IntegrationTestFactory.CrearOrdenServices(context);

            var resultado = await ordenServices.CambiarEstadoOrden(new CambiarEstadoOrdenDto
            {
                Id = 9999,
                NuevoEstado = nameof(EstadoOrden.Cancelada)
            }, escenario.Usuario.Id, esAdministrador: true);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be("Orden no encontrada");
            (await context.Notificaciones.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task CambiarEstadoOrden_ConEstadoNoReconocido_DevuelveFailureDelValidatorSinLlegarAlDominio()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenPendienteAsync(context);
            var ordenServices = IntegrationTestFactory.CrearOrdenServices(context);

            // Sin esta validación previa, el Enum.Parse del servicio reventaría con
            // ArgumentException y el cliente vería un 500 en vez de un 400.
            var resultado = await ordenServices.CambiarEstadoOrden(new CambiarEstadoOrdenDto
            {
                Id = escenario.OrdenId,
                NuevoEstado = "Despachada"
            }, escenario.Usuario.Id, esAdministrador: true);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("El estado debe ser uno de:");
            (await context.Ordenes.FindAsync(escenario.OrdenId))!.Estado.Should().Be(EstadoOrden.Pendiente);
        }

        [Fact]
        public async Task CambiarEstadoOrden_ConIdCero_DevuelveFailureDelValidator()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var ordenServices = IntegrationTestFactory.CrearOrdenServices(context);

            // No hay nada sembrado: la llamada muere en el validator del DTO, mucho antes del
            // guard de propiedad, así que el usuario solicitante da igual.
            var resultado = await ordenServices.CambiarEstadoOrden(new CambiarEstadoOrdenDto
            {
                Id = 0,
                NuevoEstado = nameof(EstadoOrden.Cancelada)
            }, usuarioSolicitanteId: 1, esAdministrador: true);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("El Id de la orden debe ser mayor que cero.");
        }

        // Regresión del IDOR: CambiarEstadoOrden era la puerta trasera que permitía lograr por
        // este endpoint lo que CancelarOrden y ConfirmarOrden ya bloqueaban por dueño.
        [Fact]
        public async Task CambiarEstadoOrden_DeOtroUsuarioSinSerAdministrador_DevuelveFailureSinTocarLaOrdenNiNotificar()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenPendienteAsync(context);
            var ordenServices = IntegrationTestFactory.CrearOrdenServices(context);

            var otroUsuario = await IntegrationTestFactory.SembrarUsuarioAsync(
                context, "Usuario Ajeno", "ajeno@test.com", "usuario.ajeno");

            var resultado = await ordenServices.CambiarEstadoOrden(new CambiarEstadoOrdenDto
            {
                Id = escenario.OrdenId,
                NuevoEstado = nameof(EstadoOrden.Cancelada)
            }, otroUsuario.Id, esAdministrador: false);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be("No tiene permiso sobre esta orden");

            (await context.Ordenes.FindAsync(escenario.OrdenId))!.Estado.Should().Be(EstadoOrden.Pendiente);
            (await context.Notificaciones.CountAsync()).Should().Be(0);
        }

        // La otra rama del guard: el administrador sí puede operar sobre órdenes ajenas.
        [Fact]
        public async Task CambiarEstadoOrden_DeOtroUsuarioSiendoAdministrador_PersisteElEstado()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var escenario = await SembrarOrdenPendienteAsync(context);
            var ordenServices = IntegrationTestFactory.CrearOrdenServices(context);

            var administrador = await IntegrationTestFactory.SembrarUsuarioAsync(
                context, "Usuario Admin", "admin@test.com", "usuario.admin");

            var resultado = await ordenServices.CambiarEstadoOrden(new CambiarEstadoOrdenDto
            {
                Id = escenario.OrdenId,
                NuevoEstado = nameof(EstadoOrden.Cancelada)
            }, administrador.Id, esAdministrador: true);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            (await context.Ordenes.FindAsync(escenario.OrdenId))!.Estado.Should().Be(EstadoOrden.Cancelada);

            // La notificación sigue yendo al dueño de la orden, no a quien ejecutó el cambio.
            (await context.Notificaciones.SingleAsync()).UsuarioId.Should().Be(escenario.Usuario.Id);
        }
    }
}
