using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductApp.Aplication.Dtos.Modulo_Productos.InventarioDto;
using ProductApp.Domian.Common.Enums.EnumsNotificacion;
using ProductApp.Domian.Common.Enums.EnumsUsuario;
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

            var resultado = await service.DescontarStockAsync(new MovimientoStockDto { ProductoId = producto.Id, Cantidad = 4 });

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

            var resultado = await service.DescontarStockAsync(new MovimientoStockDto { ProductoId = producto.Id, Cantidad = 1 });

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            (await context.Notificaciones.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task DescontarStockAsync_SinAdministradoresActivos_NoRevientaYDescuentaIgual()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var (_, producto, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 5);
            var service = IntegrationTestFactory.CrearInventarioService(context);

            var resultado = await service.DescontarStockAsync(new MovimientoStockDto { ProductoId = producto.Id, Cantidad = 4 });

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            (await context.Notificaciones.CountAsync()).Should().Be(0);
        }

        [Fact]
        public async Task DescontarStockAsync_ConCantidadMayorAlDisponible_DevuelveFailureYNoTocaElStock()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var (_, producto, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 5);
            var service = IntegrationTestFactory.CrearInventarioService(context);

            var resultado = await service.DescontarStockAsync(new MovimientoStockDto { ProductoId = producto.Id, Cantidad = 10 });

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

            var resultado = await service.DescontarStockAsync(new MovimientoStockDto { ProductoId = producto.Id, Cantidad = 1 });

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("producto inactivo");
        }

        [Fact]
        public async Task AgregarStockAsync_ConCantidadCero_DevuelveFailureDeValidacion()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var (_, producto, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 5);
            var service = IntegrationTestFactory.CrearInventarioService(context);

            var resultado = await service.AgregarStockAsync(new MovimientoStockDto { ProductoId = producto.Id, Cantidad = 0 });

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("Error de validación");
        }

        [Fact]
        public async Task AgregarStockAsync_DeUnProductoInexistente_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearInventarioService(context);

            var resultado = await service.AgregarStockAsync(new MovimientoStockDto { ProductoId = 999, Cantidad = 5 });

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
    }
}
