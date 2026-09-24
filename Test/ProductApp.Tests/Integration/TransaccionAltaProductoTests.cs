using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using ProductApp.Aplication.Dtos.ProductoDto;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Contex;
using Xunit;

namespace ProductApp.Tests.Integration
{
    // M12: el alta de un producto escribe en dos tablas (Producto e Inventario). Sin transacción,
    // un fallo en el segundo insert dejaba un producto sin inventario: no se puede vender ni
    // ajustar. El proveedor en memoria no revierte nada (ver GestorTransaccionesFake), así que
    // lo verificable acá es el protocolo: una sola transacción, commit después de los dos
    // inserts, y cero commits si algo falla en el medio.
    public class TransaccionAltaProductoTests
    {
        private static async Task<Categoria> SembrarCategoriaAsync(AppDbContext context)
        {
            var categoria = new Categoria("Categoria Test", "Descripcion de la categoria");
            context.Categorias.Add(categoria);
            await context.SaveChangesAsync();
            return categoria;
        }

        private static CreateProductoDto Dto(int categoriaId, string nombre = "Producto Nuevo")
            => new() { Nombre = nombre, Descripcion = "Descripción", Precio = 100m, Costo = 50m, CategoriaId = categoriaId };

        [Fact]
        public async Task CreateAsync_AbreUnaSolaTransaccionYLaConfirmaConLosDosInserts()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var gestor = new GestorTransaccionesFake();
            var service = IntegrationTestFactory.CrearProductoServices(context, gestorTransacciones: gestor);

            var resultado = await service.CreateAsync(Dto(categoria.Id));

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            gestor.TransaccionesIniciadas.Should().Be(1);
            gestor.CommitsConfirmados.Should().Be(1);
            (await context.Productos.CountAsync()).Should().Be(1);
            (await context.Inventario.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task CreateAsync_SiFallaElInsertDelInventario_NoConfirmaLaTransaccion()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var gestor = new GestorTransaccionesFake();

            // Único punto de fallo simulado: la segunda escritura de la operación. Con el
            // repositorio real no hay forma de provocarlo sin tocar código de producción.
            var inventarioRepo = new Mock<IInventarioRepository>();
            inventarioRepo
                .Setup(r => r.CreateAsync(It.IsAny<Inventario>()))
                .ThrowsAsync(new InvalidOperationException("fallo simulado al crear el inventario"));

            var service = IntegrationTestFactory.CrearProductoServices(
                context,
                gestorTransacciones: gestor,
                inventarioRepository: inventarioRepo.Object);

            var acto = async () => await service.CreateAsync(Dto(categoria.Id));

            await acto.Should().ThrowAsync<InvalidOperationException>();
            gestor.TransaccionesIniciadas.Should().Be(1);
            // Lo que prueba el arreglo: el insert del producto quedó dentro de una transacción que
            // nunca se confirmó. Contra SQL Server eso es exactamente "no quedó producto huérfano".
            // No se afirma nada sobre context.Productos porque InMemory no revierte.
            gestor.CommitsConfirmados.Should().Be(0);
            gestor.TransaccionesLiberadas.Should().Be(1);
        }

        [Fact]
        public async Task CreateAsync_ConNombreDuplicado_ValidaDentroDeLaTransaccionYNoConfirma()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            await IntegrationTestFactory.CrearProductoServices(context).CreateAsync(Dto(categoria.Id, "Repetido"));

            var gestor = new GestorTransaccionesFake();
            var service = IntegrationTestFactory.CrearProductoServices(context, gestorTransacciones: gestor);

            var resultado = await service.CreateAsync(Dto(categoria.Id, "Repetido"));

            resultado.IsSuccess.Should().BeFalse();
            // El duplicado se comprueba DENTRO de la transacción: se abre y se suelta sin commit.
            // Si algún día se moviera afuera, TransaccionesIniciadas daría 0 y este test avisaría.
            gestor.TransaccionesIniciadas.Should().Be(1);
            gestor.CommitsConfirmados.Should().Be(0);
            (await context.Productos.CountAsync()).Should().Be(1);
            (await context.Inventario.CountAsync()).Should().Be(1);
        }
    }
}
