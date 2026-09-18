using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductApp.Aplication.Dtos.ProductoDto;
using ProductApp.Domian.Entitis;
using Xunit;

namespace ProductApp.Tests.Integration
{
    public class ProductoServiceCreateTests
    {
        private static async Task<Categoria> SembrarCategoriaAsync(Infraesctructura.Persistencia.Contex.AppDbContext context)
        {
            var categoria = new Categoria("Categoria Test", "Descripcion de la categoria");
            context.Categorias.Add(categoria);
            await context.SaveChangesAsync();
            return categoria;
        }

        private static CreateProductoDto Dto(int categoriaId, string nombre = "Producto Nuevo")
            => new() { Nombre = nombre, Descripcion = "Descripción", Precio = 100m, Costo = 50m, CategoriaId = categoriaId };

        [Fact]
        public async Task CreateAsync_TomaLaCantidadMinimaDeLaConfiguracionDelSistema()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            context.ConfiguracionSistema.Add(new ConfiguracionSistema(12, 60, "Empresa Test", "DOP"));
            await context.SaveChangesAsync();
            var service = IntegrationTestFactory.CrearProductoServices(context);

            var resultado = await service.CreateAsync(Dto(categoria.Id));

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            var inventario = await context.Inventario.SingleAsync();
            inventario.CantidadMinima.Should().Be(12);
            inventario.CantidadActual.Should().Be(0);
        }

        [Fact]
        public async Task CreateAsync_SinConfiguracionEnBase_UsaElRespaldoDeCincoUnidades()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var service = IntegrationTestFactory.CrearProductoServices(context);

            var resultado = await service.CreateAsync(Dto(categoria.Id));

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            (await context.Inventario.SingleAsync()).CantidadMinima.Should().Be(5);
        }

        [Fact]
        public async Task CreateAsync_ConNombreDuplicado_NoDejaProductoNiInventarioHuerfano()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var service = IntegrationTestFactory.CrearProductoServices(context);
            await service.CreateAsync(Dto(categoria.Id, "Repetido"));

            var resultado = await service.CreateAsync(Dto(categoria.Id, "Repetido"));

            resultado.IsSuccess.Should().BeFalse();
            (await context.Productos.CountAsync()).Should().Be(1);
            (await context.Inventario.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task CreateAsync_ConPrecioCero_DevuelveFailureDeValidacion()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var service = IntegrationTestFactory.CrearProductoServices(context);
            var dto = Dto(categoria.Id);
            dto.Precio = 0;

            var resultado = await service.CreateAsync(dto);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("precio debe ser mayor a 0");
        }

        [Fact]
        public async Task CreateAsync_ConProveedorId_LoGuardaEnElProducto()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var proveedor = await IntegrationTestFactory.SembrarProveedorAsync(context);
            var service = IntegrationTestFactory.CrearProductoServices(context);
            var dto = Dto(categoria.Id);
            dto.ProveedorId = proveedor.Id;

            var resultado = await service.CreateAsync(dto);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            (await context.Productos.SingleAsync()).ProveedorId.Should().Be(proveedor.Id);
        }
    }
}
