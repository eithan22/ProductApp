using FluentAssertions;
using Moq;
using ProductApp.Aplication.Dtos.Modulo_Ventas.DetalleOrdenDto;
using ProductApp.Aplication.Dtos.OrdenDto;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Contex;
using ProductApp.Infraesctructura.Persistencia.Repository;
using Xunit;

namespace ProductApp.Tests.Integration
{
    // M3: cada operación sobre el detalle escribe la fila del detalle y además recalcula
    // Orden.Total. Son dos escrituras: si la segunda no ocurre, la orden se cobra y se factura
    // por un monto que no corresponde a sus productos. Igual que en TransaccionAltaProductoTests,
    // con InMemory se verifica el protocolo (una transacción, commit al final, cero commits si
    // falla el recálculo), no el rollback físico.
    public class TransaccionDetalleOrdenTests
    {
        private static async Task<(int OrdenId, Usuario Usuario, Producto Producto)> SembrarOrdenPendienteAsync(AppDbContext context)
        {
            var (_, producto, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 20, precio: 20);
            var cliente = await IntegrationTestFactory.SembrarClienteAsync(context);
            var usuario = await IntegrationTestFactory.SembrarUsuarioAsync(context);

            var crear = await IntegrationTestFactory.CrearOrdenServices(context)
                .CrearOrden(new CreateOrdenDto { ClienteId = cliente.Id }, usuario.Id);
            crear.IsSuccess.Should().BeTrue(crear.Message);

            return (crear.Data!.Id, usuario, producto);
        }

        [Fact]
        public async Task AgregarProductoAsync_EscribeElDetalleYRecalculaElTotalEnUnaSolaTransaccion()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var (ordenId, usuario, producto) = await SembrarOrdenPendienteAsync(context);
            var gestor = new GestorTransaccionesFake();
            var service = IntegrationTestFactory.CrearDetalleOrdenService(context, gestor);

            var resultado = await service.AgregarProductoAsync(
                new CreateDetalleOrdenDto { OrdenId = ordenId, ProductId = producto.Id, Cantidad = 2 },
                usuario.Id, esAdministrador: false);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            gestor.TransaccionesIniciadas.Should().Be(1);
            gestor.CommitsConfirmados.Should().Be(1);
            (await context.Ordenes.FindAsync(ordenId))!.Total.Should().Be(40m);
        }

        [Fact]
        public async Task AgregarProductoAsync_SiFallaElRecalculoDelTotal_NoConfirmaLaTransaccion()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var (ordenId, usuario, producto) = await SembrarOrdenPendienteAsync(context);
            var gestor = new GestorTransaccionesFake();

            // Repositorio de órdenes con una sola pieza rota: el UpdateAsync del recálculo, que es
            // la segunda escritura. Las lecturas se delegan al repositorio real para que el guard
            // de propiedad y el validator de negocio se comporten igual que en producción.
            var real = new OrdenRepository(context);
            var ordenRepo = new Mock<IOrdenRepository>();
            ordenRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>())).Returns((int id) => real.GetByIdAsync(id));
            ordenRepo.Setup(r => r.UpdateAsync(It.IsAny<Orden>()))
                     .ThrowsAsync(new InvalidOperationException("fallo simulado al recalcular el total"));

            var service = IntegrationTestFactory.CrearDetalleOrdenService(context, gestor, ordenRepo.Object);

            var acto = async () => await service.AgregarProductoAsync(
                new CreateDetalleOrdenDto { OrdenId = ordenId, ProductId = producto.Id, Cantidad = 2 },
                usuario.Id, esAdministrador: false);

            await acto.Should().ThrowAsync<InvalidOperationException>();
            gestor.TransaccionesIniciadas.Should().Be(1);
            // El detalle se insertó dentro de una transacción sin commit: contra SQL Server no
            // queda detalle sin su total recalculado. No se afirma nada sobre el Total leído del
            // mismo contexto porque InMemory devuelve la misma instancia trackeada que
            // RecalcularTotalOrdenAsync ya mutó en memoria antes de que UpdateAsync fallara — el
            // valor en memoria no refleja si hubo o no persistencia real.
            gestor.CommitsConfirmados.Should().Be(0);
        }
    }
}
