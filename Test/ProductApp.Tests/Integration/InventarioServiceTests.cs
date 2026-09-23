using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductApp.Aplication.Dtos.Modulo_Productos.InventarioDto;
using ProductApp.Domian.Common.Enums.EnumsNotificacion;
using ProductApp.Domian.Common.Enums.EnumsUsuario;
using ProductApp.Domian.Common.Exceptions;
using ProductApp.Domian.Entitis;
using Xunit;

namespace ProductApp.Tests.Integration
{
    public class InventarioServiceTests
    {
        private static async Task<Usuario> SembrarAdministradorAsync(Infraesctructura.Persistencia.Contex.AppDbContext context)
        {
            var admin = new Usuario("Admin Test", "admin@test.com", "admin.test", RolUsuario.Administrador);
            context.Usuarios.Add(admin);
            await context.SaveChangesAsync();
            return admin;
        }

        [Fact]
        public async Task DescontarStockAsync_CuandoElStockCruzaElMinimo_NotificaAlosAdministradores()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            await SembrarAdministradorAsync(context);
            var (_, producto, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 5);
            var service = IntegrationTestFactory.CrearInventarioService(context);

            var resultado = await service.DescontarStockAsync(new MovimientoStockDto { ProductoId = producto.Id, Cantidad = 4 }, usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            (await context.Notificaciones.CountAsync()).Should().Be(1);
            var notificacion = await context.Notificaciones.SingleAsync();
            notificacion.Tipo.Should().Be(TipoNotificacion.StockBajo);
            notificacion.Mensaje.Should().Contain("Producto Test");
        }

        [Fact]
        public async Task DescontarStockAsync_CuandoYaEstabaEnStockBajo_NoVuelveANotificar()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            await SembrarAdministradorAsync(context);
            // SembrarProductoConInventarioAsync fija la cantidad mínima en 1: con cantidadActual: 1
            // ya nace en stock bajo (estabaBajo será true ANTES de este descuento).
            var (_, producto, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 1);
            var service = IntegrationTestFactory.CrearInventarioService(context);

            var resultado = await service.DescontarStockAsync(new MovimientoStockDto { ProductoId = producto.Id, Cantidad = 1 }, usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            (await context.Notificaciones.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task DescontarStockAsync_SinAdministradoresActivos_NoRevientaYDescuentaIgual()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var (_, producto, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 5);
            var service = IntegrationTestFactory.CrearInventarioService(context);

            var resultado = await service.DescontarStockAsync(new MovimientoStockDto { ProductoId = producto.Id, Cantidad = 4 }, usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            (await context.Notificaciones.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task DescontarStockAsync_ConCantidadMayorAlDisponible_DevuelveFailureYNoTocaElStock()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var (_, producto, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 5);
            var service = IntegrationTestFactory.CrearInventarioService(context);

            var resultado = await service.DescontarStockAsync(new MovimientoStockDto { ProductoId = producto.Id, Cantidad = 10 }, usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("Stock insuficiente");
            (await context.Inventario.SingleAsync()).CantidadActual.Should().Be(5);
        }

        [Fact]
        public async Task DescontarStockAsync_DeUnProductoInactivo_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var (_, producto, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 5);
            producto.DesactivarProducto();
            await context.SaveChangesAsync();
            var service = IntegrationTestFactory.CrearInventarioService(context);

            var resultado = await service.DescontarStockAsync(new MovimientoStockDto { ProductoId = producto.Id, Cantidad = 1 }, usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("producto inactivo");
        }

        [Fact]
        public async Task AgregarStockAsync_ConCantidadCero_DevuelveFailureDeValidacion()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var (_, producto, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 5);
            var service = IntegrationTestFactory.CrearInventarioService(context);

            var resultado = await service.AgregarStockAsync(new MovimientoStockDto { ProductoId = producto.Id, Cantidad = 0 }, usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("Error de validación");
        }

        [Fact]
        public async Task AgregarStockAsync_DeUnProductoInexistente_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearInventarioService(context);

            var resultado = await service.AgregarStockAsync(new MovimientoStockDto { ProductoId = 999, Cantidad = 5 }, usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be("Inventario no encontrado.");
        }

        [Fact]
        public async Task AjustarStockAsync_SubiendoElMinimoPorEncimaDelActual_DejaStockBajoYNotifica()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            await SembrarAdministradorAsync(context);
            var (_, producto, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 10);
            var service = IntegrationTestFactory.CrearInventarioService(context);

            var resultado = await service.AjustarStockAsync(
                new AjustarStockDto { ProductoId = producto.Id, NuevoStock = 10, NuevoStockMinimo = 20 },
                usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            (await context.Notificaciones.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task AjustarStockAsync_ConNuevoStockNegativo_DevuelveFailureDeValidacion()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var (_, producto, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 10);
            var service = IntegrationTestFactory.CrearInventarioService(context);

            var resultado = await service.AjustarStockAsync(
                new AjustarStockDto { ProductoId = producto.Id, NuevoStock = -1 },
                usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task ObtenerStockBajoAsync_ConProveedorId_SoloDevuelveLosDeEseProveedor()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var proveedor = await IntegrationTestFactory.SembrarProveedorAsync(context);
            var (_, productoConProveedor, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 1);
            productoConProveedor.AsignarProveedor(proveedor.Id);
            await context.SaveChangesAsync();
            await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 1);
            var service = IntegrationTestFactory.CrearInventarioService(context);

            var resultado = await service.ObtenerStockBajoAsync(proveedor.Id);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task ObtenerTodosInventariosAsync_ConPageSize101_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearInventarioService(context);

            var resultado = await service.ObtenerTodosInventariosAsync(pageSize: 101);

            resultado.IsSuccess.Should().BeFalse();
        }

        // --- AgregarStockAsync: camino feliz (antes sin cobertura) ---

        [Fact]
        public async Task AgregarStockAsync_ConCantidadValida_SumaElStockYLoPersiste()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var (_, producto, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 5);
            var service = IntegrationTestFactory.CrearInventarioService(context);

            var resultado = await service.AgregarStockAsync(new MovimientoStockDto { ProductoId = producto.Id, Cantidad = 7 }, usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Message.Should().Be("Stock agregado exitosamente.");
            (await context.Inventario.SingleAsync()).CantidadActual.Should().Be(12);
        }

        [Fact]
        public async Task AgregarStockAsync_DevuelveElInventarioYaActualizadoEnElDto()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var (_, producto, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 5);
            var service = IntegrationTestFactory.CrearInventarioService(context);

            var resultado = await service.AgregarStockAsync(new MovimientoStockDto { ProductoId = producto.Id, Cantidad = 3 }, usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data.Should().NotBeNull();
            resultado.Data!.StockActual.Should().Be(8);
            resultado.Data.ProductoId.Should().Be(producto.Id);
            resultado.Data.Producto.Should().Be("Producto Test");
            resultado.Data.StockMinimo.Should().Be(1);
        }

        [Fact]
        public async Task AgregarStockAsync_DosEntradasSeguidas_AcumulanSobreElMismoInventario()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var (_, producto, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 0);
            var service = IntegrationTestFactory.CrearInventarioService(context);

            await service.AgregarStockAsync(new MovimientoStockDto { ProductoId = producto.Id, Cantidad = 4 }, usuarioSolicitanteId: 1);
            var resultado = await service.AgregarStockAsync(new MovimientoStockDto { ProductoId = producto.Id, Cantidad = 6 }, usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.StockActual.Should().Be(10);
            (await context.Inventario.SingleAsync()).CantidadActual.Should().Be(10);
        }

        // El delay hace determinista la comparación de timestamps: sin él, dos lecturas
        // consecutivas de DateTime.UtcNow pueden devolver el mismo valor.
        [Fact]
        public async Task AgregarStockAsync_RefrescaLaFechaDeUltimaActualizacion()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var (_, producto, inventario) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 5);
            var fechaPrevia = inventario.UltimaActualizacion;
            var service = IntegrationTestFactory.CrearInventarioService(context);
            await Task.Delay(10);

            var resultado = await service.AgregarStockAsync(new MovimientoStockDto { ProductoId = producto.Id, Cantidad = 2 }, usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            (await context.Inventario.SingleAsync()).UltimaActualizacion.Should().BeAfter(fechaPrevia);
            resultado.Data!.FechaActualizacion.Should().BeAfter(fechaPrevia);
        }

        // Reponer stock saca al producto de stock bajo; la notificación solo se dispara al
        // CRUZAR hacia abajo, así que aquí no debe generarse ninguna.
        [Fact]
        public async Task AgregarStockAsync_SobreUnProductoEnStockBajo_LoSacaDeStockBajoYNoNotifica()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            await SembrarAdministradorAsync(context);
            var (_, producto, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 1);
            var service = IntegrationTestFactory.CrearInventarioService(context);

            var resultado = await service.AgregarStockAsync(new MovimientoStockDto { ProductoId = producto.Id, Cantidad = 10 }, usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            var inventario = await context.Inventario.SingleAsync();
            inventario.CantidadActual.Should().Be(11);
            inventario.EsStockBajo().Should().BeFalse();
            (await context.Notificaciones.CountAsync()).Should().Be(0);
        }

        // --- AgregarStockAsync: caminos de error ---

        [Fact]
        public async Task AgregarStockAsync_DeUnProductoInactivo_DevuelveFailureYNoTocaElStock()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var (_, producto, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 5);
            producto.DesactivarProducto();
            await context.SaveChangesAsync();
            var service = IntegrationTestFactory.CrearInventarioService(context);

            var resultado = await service.AgregarStockAsync(new MovimientoStockDto { ProductoId = producto.Id, Cantidad = 10 }, usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be("No se puede agregar stock a un producto inactivo.");
            (await context.Inventario.SingleAsync()).CantidadActual.Should().Be(5);
        }

        [Fact]
        public async Task AgregarStockAsync_ConCantidadNegativa_DevuelveFailureDeValidacionYNoTocaElStock()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var (_, producto, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 5);
            var service = IntegrationTestFactory.CrearInventarioService(context);

            var resultado = await service.AgregarStockAsync(new MovimientoStockDto { ProductoId = producto.Id, Cantidad = -3 }, usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("Error de validación");
            (await context.Inventario.SingleAsync()).CantidadActual.Should().Be(5);
        }

        [Fact]
        public async Task AgregarStockAsync_ConProductoIdCero_DevuelveFailureDeValidacion()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearInventarioService(context);

            var resultado = await service.AgregarStockAsync(new MovimientoStockDto { ProductoId = 0, Cantidad = 5 }, usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("Error de validación");
        }

        // El producto existe pero su inventario fue borrado lógicamente: no hay dónde sumar.
        [Fact]
        public async Task AgregarStockAsync_ConInventarioEliminadoLogicamente_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var (_, producto, inventario) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 5);
            inventario.Eliminar();
            await context.SaveChangesAsync();
            var service = IntegrationTestFactory.CrearInventarioService(context);

            var resultado = await service.AgregarStockAsync(new MovimientoStockDto { ProductoId = producto.Id, Cantidad = 5 }, usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be("Inventario no encontrado.");
        }

        // --- AgregarStockAsync: tope máximo de stock ---

        // La cantidad supera el tope por sí sola, así que FluentValidation corta antes de
        // llegar al dominio: sale un Failure de validación, no una excepción.
        [Fact]
        public async Task AgregarStockAsync_ConCantidadSuperiorAlMaximoPermitido_DevuelveFailureDeValidacionYNoTocaElStock()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var (_, producto, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 5);
            var service = IntegrationTestFactory.CrearInventarioService(context);

            var resultado = await service.AgregarStockAsync(
                new MovimientoStockDto { ProductoId = producto.Id, Cantidad = Inventario.CantidadMaximaStock + 1 },
                usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("Error de validación");
            resultado.Message.Should().Contain("no puede superar");
            (await context.Inventario.SingleAsync()).CantidadActual.Should().Be(5);
        }

        // Caso distinto: la cantidad pasa FluentValidation (no supera el tope por sí sola),
        // pero la SUMA sí lo cruza. Ahí corta la entidad y la excepción de dominio sale del
        // servicio sin convertirse en Failure; la API la traduce a 400 en GlobalExceptionHandler.
        [Fact]
        public async Task AgregarStockAsync_CuandoLaSumaCruzaElTope_PropagaLaExcepcionDeDominioYNoTocaElStock()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var (_, producto, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(
                context, cantidadActual: Inventario.CantidadMaximaStock - 10);
            var service = IntegrationTestFactory.CrearInventarioService(context);

            var accion = async () => await service.AgregarStockAsync(
                new MovimientoStockDto { ProductoId = producto.Id, Cantidad = 11 },
                usuarioSolicitanteId: 1);

            await accion.Should().ThrowAsync<ValidacionDominioException>()
                .WithMessage("*Stock máximo excedido*");
            (await context.Inventario.SingleAsync()).CantidadActual.Should().Be(Inventario.CantidadMaximaStock - 10);
        }
    }
}
