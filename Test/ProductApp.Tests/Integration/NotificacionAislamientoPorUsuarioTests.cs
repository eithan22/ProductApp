using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductApp.Domian.Common.Base;
using ProductApp.Domian.Common.Enums.EnumsNotificacion;
using ProductApp.Domian.Common.Enums.EnumsUsuario;
using ProductApp.Domian.Entitis;
using ProductApp.Infraesctructura.Persistencia.Contex;
using Xunit;

namespace ProductApp.Tests.Integration
{
    // El aislamiento entre usuarios de las notificaciones vive mitad en el servicio
    // (MarcarComoLeidaAsync compara UsuarioId) y mitad en las consultas EF del repositorio
    // (MarcarTodasComoLeidasAsync, ObtenerRecientesPorUsuarioAsync, ContarNoLeidasPorUsuarioAsync).
    // Esa segunda mitad solo se puede verificar de verdad contra un DbContext real.
    public class NotificacionAislamientoPorUsuarioTests
    {
        private static async Task<Usuario> SembrarUsuarioAsync(AppDbContext context, string username)
        {
            var usuario = new Usuario($"Usuario {username}", $"{username}@test.com", username, RolUsuario.Vendedor);
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();
            return usuario;
        }

        private static Notificacion Nueva(int usuarioId, string mensaje, bool leida = false, DateTime? creadoEn = null)
        {
            var notificacion = new Notificacion(usuarioId, TipoNotificacion.StockBajo, mensaje);

            if (leida)
                notificacion.MarcarComoLeida();

            // CreadoEn y ModificadoEn se declaran en BaseEntity con setter privado: se fuerzan por
            // reflexión para que el orden y las fechas del test sean deterministas y no dependan
            // de la resolución del reloj. Se hace ANTES de guardar: SaveChangesAsync solo toca
            // ModificadoEn de las entidades en estado Modified, no de las Added.
            if (creadoEn.HasValue)
                typeof(BaseEntity).GetProperty(nameof(BaseEntity.CreadoEn))!.SetValue(notificacion, creadoEn.Value);

            return notificacion;
        }

        [Fact]
        public async Task MarcarComoLeidaAsync_ConNotificacionDeOtroUsuario_NoLaMarcaEnBaseDeDatos()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var duenio = await SembrarUsuarioAsync(context, "duenio");
            var intruso = await SembrarUsuarioAsync(context, "intruso");
            var notificacion = Nueva(duenio.Id, "Stock bajo en Teclado.");
            context.Notificaciones.Add(notificacion);
            await context.SaveChangesAsync();
            var service = IntegrationTestFactory.CrearNotificacionService(context);

            var resultado = await service.MarcarComoLeidaAsync(notificacion.Id, intruso.Id);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be("No autorizado para modificar esta notificación");
            var enBd = await context.Notificaciones.AsNoTracking().SingleAsync(n => n.Id == notificacion.Id);
            enBd.Leida.Should().BeFalse("la notificación es de otro usuario y no debió tocarse");
        }

        [Fact]
        public async Task MarcarComoLeidaAsync_ConNotificacionPropia_QuedaLeidaEnBaseDeDatos()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var duenio = await SembrarUsuarioAsync(context, "duenio");
            var notificacion = Nueva(duenio.Id, "Stock bajo en Teclado.");
            context.Notificaciones.Add(notificacion);
            await context.SaveChangesAsync();
            var service = IntegrationTestFactory.CrearNotificacionService(context);

            var resultado = await service.MarcarComoLeidaAsync(notificacion.Id, duenio.Id);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            var enBd = await context.Notificaciones.AsNoTracking().SingleAsync(n => n.Id == notificacion.Id);
            enBd.Leida.Should().BeTrue();
        }

        [Fact]
        public async Task MarcarComoLeidaAsync_ConNotificacionEliminada_DevuelveNoEncontrada()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var duenio = await SembrarUsuarioAsync(context, "duenio");
            var notificacion = Nueva(duenio.Id, "Stock bajo en Teclado.");
            notificacion.Eliminar();
            context.Notificaciones.Add(notificacion);
            await context.SaveChangesAsync();
            var service = IntegrationTestFactory.CrearNotificacionService(context);

            var resultado = await service.MarcarComoLeidaAsync(notificacion.Id, duenio.Id);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be("Notificación no encontrada");
        }

        // Documenta el comportamiento real: MarcarComoLeida() es idempotente en la entidad
        // (early return, no lanza), pero el servicio llama a UpdateAsync igual, y Update() marca
        // la fila como Modified, asi que SaveChangesAsync le mueve ModificadoEn sin que nada
        // haya cambiado. Escritura innecesaria, no un fallo funcional.
        [Fact]
        public async Task MarcarComoLeidaAsync_SobreUnaNotificacionYaLeida_DevuelveSuccessPeroReescribeLaFila()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var duenio = await SembrarUsuarioAsync(context, "duenio");
            var notificacion = Nueva(duenio.Id, "Stock bajo en Teclado.", leida: true);
            var modificadoOriginal = DateTime.UtcNow.AddDays(-1);
            typeof(BaseEntity).GetProperty(nameof(BaseEntity.ModificadoEn))!.SetValue(notificacion, modificadoOriginal);
            context.Notificaciones.Add(notificacion);
            await context.SaveChangesAsync();
            var service = IntegrationTestFactory.CrearNotificacionService(context);

            var resultado = await service.MarcarComoLeidaAsync(notificacion.Id, duenio.Id);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            var enBd = await context.Notificaciones.AsNoTracking().SingleAsync(n => n.Id == notificacion.Id);
            enBd.Leida.Should().BeTrue();
            enBd.ModificadoEn.Should().BeAfter(modificadoOriginal);
        }

        [Fact]
        public async Task MarcarTodasComoLeidasAsync_SoloAfectaLasNotificacionesDelUsuarioAutenticado()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var duenio = await SembrarUsuarioAsync(context, "duenio");
            var otro = await SembrarUsuarioAsync(context, "otro");
            context.Notificaciones.AddRange(
                Nueva(duenio.Id, "Propia 1"),
                Nueva(duenio.Id, "Propia 2"),
                Nueva(otro.Id, "Ajena 1"),
                Nueva(otro.Id, "Ajena 2"));
            await context.SaveChangesAsync();
            var service = IntegrationTestFactory.CrearNotificacionService(context);

            var resultado = await service.MarcarTodasComoLeidasAsync(duenio.Id);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            var enBd = await context.Notificaciones.AsNoTracking().ToListAsync();
            enBd.Where(n => n.UsuarioId == duenio.Id).Should().OnlyContain(n => n.Leida);
            enBd.Where(n => n.UsuarioId == otro.Id).Should().OnlyContain(n => !n.Leida);
        }

        [Fact]
        public async Task ObtenerResumenAsync_SoloDevuelveYCuentaLasNotificacionesDelUsuario()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var duenio = await SembrarUsuarioAsync(context, "duenio");
            var otro = await SembrarUsuarioAsync(context, "otro");
            context.Notificaciones.AddRange(
                Nueva(duenio.Id, "Propia no leída"),
                Nueva(duenio.Id, "Propia leída", leida: true),
                Nueva(otro.Id, "Ajena no leída"),
                Nueva(otro.Id, "Otra ajena no leída"));
            await context.SaveChangesAsync();
            var service = IntegrationTestFactory.CrearNotificacionService(context);

            var resultado = await service.ObtenerResumenAsync(duenio.Id);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.Recientes.Should().HaveCount(2);
            resultado.Data.Recientes.Select(n => n.Mensaje).Should().NotContain(m => m.Contains("Ajena"));
            resultado.Data.NoLeidas.Should().Be(1, "solo cuenta las no leídas del propio usuario");
        }

        [Fact]
        public async Task ObtenerResumenAsync_DevuelveLasMasRecientesPrimeroYRespetaElLimite()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var duenio = await SembrarUsuarioAsync(context, "duenio");
            var ahora = DateTime.UtcNow;
            context.Notificaciones.AddRange(
                Nueva(duenio.Id, "Más vieja", creadoEn: ahora.AddHours(-3)),
                Nueva(duenio.Id, "Intermedia", creadoEn: ahora.AddHours(-2)),
                Nueva(duenio.Id, "Más nueva", creadoEn: ahora.AddHours(-1)));
            await context.SaveChangesAsync();
            var service = IntegrationTestFactory.CrearNotificacionService(context);

            var resultado = await service.ObtenerResumenAsync(duenio.Id, cantidad: 2);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.Recientes.Select(n => n.Mensaje).Should().Equal("Más nueva", "Intermedia");
            resultado.Data.NoLeidas.Should().Be(3, "el contador no se recorta por el límite de recientes");
        }

        [Fact]
        public async Task ObtenerResumenAsync_SinNotificaciones_DevuelveResumenVacioYNoFalla()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var duenio = await SembrarUsuarioAsync(context, "duenio");
            var service = IntegrationTestFactory.CrearNotificacionService(context);

            var resultado = await service.ObtenerResumenAsync(duenio.Id);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.Recientes.Should().BeEmpty();
            resultado.Data.NoLeidas.Should().Be(0);
        }

        [Fact]
        public async Task NotificarAdministradoresAsync_SoloAlcanzaAAdministradoresActivos()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var adminActivo = new Usuario("Admin Activo", "admin.activo@test.com", "admin.activo", RolUsuario.Administrador);
            var adminInactivo = new Usuario("Admin Inactivo", "admin.inactivo@test.com", "admin.inactivo", RolUsuario.Administrador);
            adminInactivo.Desactivar();
            var vendedor = new Usuario("Vendedor", "vendedor@test.com", "vendedor", RolUsuario.Vendedor);
            context.Usuarios.AddRange(adminActivo, adminInactivo, vendedor);
            await context.SaveChangesAsync();
            var service = IntegrationTestFactory.CrearNotificacionService(context);

            await service.NotificarAdministradoresAsync(TipoNotificacion.StockBajo, "Stock bajo en Teclado.");

            var enBd = await context.Notificaciones.AsNoTracking().ToListAsync();
            enBd.Should().ContainSingle();
            enBd[0].UsuarioId.Should().Be(adminActivo.Id);
        }
    }
}
