using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductApp.Aplication.Dtos.CategoriaDto;
using ProductApp.Domian.Common.Exceptions;
using ProductApp.Domian.Entitis;
using ProductApp.Infraesctructura.Persistencia.Contex;
using Xunit;

namespace ProductApp.Tests.Integration
{
    public class CategoriaServiceTests
    {
        private static CreateCategoriaDto CrearDto(
            string nombre = "Ferretería",
            string descripcion = "Herramientas y tornillería")
            => new() { Nombre = nombre, Descripcion = descripcion };

        private static async Task<Categoria> SembrarCategoriaAsync(
            AppDbContext context,
            string nombre = "Ferretería",
            string descripcion = "Herramientas y tornillería")
        {
            var categoria = new Categoria(nombre, descripcion);
            context.Categorias.Add(categoria);
            await context.SaveChangesAsync();
            return categoria;
        }

        [Fact]
        public async Task CreateAsync_ConDatosValidos_GuardaLaCategoriaActiva()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearCategoriaServices(context);

            var resultado = await service.CreateAsync(CrearDto());

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.Estado.Should().Be("Activo");
            (await context.Categorias.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task CreateAsync_ConNombreDuplicado_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            await SembrarCategoriaAsync(context, "Ferretería", "Otra descripción distinta");
            var service = IntegrationTestFactory.CrearCategoriaServices(context);

            var resultado = await service.CreateAsync(CrearDto());

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("Nombre ya existe");
            (await context.Categorias.CountAsync()).Should().Be(1);
        }

        // B9 (auditoría de seguridad): la descripción NO se valida como única — dos
        // categorías distintas pueden compartir texto descriptivo ("Productos varios").
        [Fact]
        public async Task CreateAsync_ConDescripcionDuplicada_LoPermite()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            await SembrarCategoriaAsync(context, "Otro nombre", "Herramientas y tornillería");
            var service = IntegrationTestFactory.CrearCategoriaServices(context);

            var resultado = await service.CreateAsync(CrearDto());

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            (await context.Categorias.CountAsync()).Should().Be(2);
        }

        // Al revés que en Proveedor: la baja de Categoría es soft delete (EstaEliminado),
        // y ExisteAsync filtra justamente por ese campo, así que desactivar SÍ libera el
        // nombre para una categoría nueva.
        [Fact]
        public async Task CreateAsync_ConElNombreDeUnaCategoriaInactiva_LoPermite()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var service = IntegrationTestFactory.CrearCategoriaServices(context);
            (await service.DisableAsync(categoria.Id)).IsSuccess.Should().BeTrue();

            var resultado = await service.CreateAsync(CrearDto());

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            (await context.Categorias.CountAsync()).Should().Be(2);
        }

        [Fact]
        public async Task GetAllAsync_DespuesDeCrear_DevuelveLaCategoriaEnElListadoPaginado()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearCategoriaServices(context);
            var creada = await service.CreateAsync(CrearDto());
            creada.IsSuccess.Should().BeTrue(creada.Message);

            var listado = await service.GetAllAsync();

            listado.IsSuccess.Should().BeTrue(listado.Message);
            listado.Data!.TotalCount.Should().Be(1);
            listado.Data.PageNumber.Should().Be(1);
            listado.Data.Items.Should().ContainSingle(c => c.Id == creada.Data!.Id && c.Nombre == "Ferretería");
        }

        [Fact]
        public async Task UpdateAsync_ConservandoSuPropioNombre_NoSeDetectaComoDuplicado()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var service = IntegrationTestFactory.CrearCategoriaServices(context);

            var resultado = await service.UpdateAsync(new UpdateCategoriaDto
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                Descripcion = "Herramientas, tornillería y pintura"
            });

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.Nombre.Should().Be("Ferretería");
            resultado.Data.Descripcion.Should().Be("Herramientas, tornillería y pintura");
        }

        [Fact]
        public async Task UpdateAsync_ConElNombreDeOtraCategoria_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            await SembrarCategoriaAsync(context, "Ferretería", "Herramientas y tornillería");
            var pinturas = await SembrarCategoriaAsync(context, "Pinturas", "Pintura y solventes");
            var service = IntegrationTestFactory.CrearCategoriaServices(context);

            var resultado = await service.UpdateAsync(new UpdateCategoriaDto
            {
                Id = pinturas.Id,
                Nombre = "Ferretería",
                Descripcion = "Pintura y solventes"
            });

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("Nombre ya existe");
            (await context.Categorias.FindAsync(pinturas.Id))!.Nombre.Should().Be("Pinturas");
        }

        // B9 (auditoría de seguridad): igual que en el create, la descripción no es única.
        [Fact]
        public async Task UpdateAsync_ConLaDescripcionDeOtraCategoria_LoPermite()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            await SembrarCategoriaAsync(context, "Ferretería", "Herramientas y tornillería");
            var pinturas = await SembrarCategoriaAsync(context, "Pinturas", "Pintura y solventes");
            var service = IntegrationTestFactory.CrearCategoriaServices(context);

            var resultado = await service.UpdateAsync(new UpdateCategoriaDto
            {
                Id = pinturas.Id,
                Nombre = "Pinturas",
                Descripcion = "Herramientas y tornillería"
            });

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            (await context.Categorias.FindAsync(pinturas.Id))!.Descripcion.Should().Be("Herramientas y tornillería");
        }

        [Fact]
        public async Task UpdateAsync_ConIdInexistente_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearCategoriaServices(context);

            var resultado = await service.UpdateAsync(new UpdateCategoriaDto
            {
                Id = 999,
                Nombre = "Cualquiera",
                Descripcion = "Cualquier descripción"
            });

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("no fue encontrada");
        }

        [Fact]
        public async Task DisableAsync_SacaALaCategoriaDelListadoPorDefectoPeroNoDeLaBase()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var service = IntegrationTestFactory.CrearCategoriaServices(context);

            var resultado = await service.DisableAsync(categoria.Id);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            (await service.GetAllAsync()).Data!.TotalCount.Should().Be(0);
            (await service.GetAllAsync(incluirInactivos: true)).Data!.TotalCount.Should().Be(1);
            (await context.Categorias.CountAsync()).Should().Be(1);
        }

        // La segunda desactivación falla, pero NO por la regla de negocio: DisableAsync
        // busca con GetByIdAsync, que ya filtra EstaEliminado, así que la categoría
        // inactiva ni siquiera llega al validador y el usuario ve "no fue encontrada"
        // en lugar de "ya está inactiva".
        [Fact]
        public async Task DisableAsync_SobreCategoriaYaInactiva_DevuelveFailureDeNoEncontrada()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var service = IntegrationTestFactory.CrearCategoriaServices(context);
            await service.DisableAsync(categoria.Id);

            var resultado = await service.DisableAsync(categoria.Id);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("no fue encontrada");
        }

        // Ninguna regla impide desactivar una categoría que todavía tiene productos
        // asociados: el producto queda apuntando a una categoría inactiva.
        [Fact]
        public async Task DisableAsync_ConProductosAsociados_DesactivaIgualYDejaElProductoActivo()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var (categoria, producto, _) = await IntegrationTestFactory.SembrarProductoConInventarioAsync(context, cantidadActual: 5);
            var service = IntegrationTestFactory.CrearCategoriaServices(context);

            var resultado = await service.DisableAsync(categoria.Id);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            (await context.Productos.FindAsync(producto.Id))!.EstaEliminado.Should().BeFalse();
        }

        [Fact]
        public async Task EnableCategoria_SobreCategoriaInactiva_LaReactiva()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var service = IntegrationTestFactory.CrearCategoriaServices(context);
            await service.DisableAsync(categoria.Id);

            var resultado = await service.EnableCategoria(categoria.Id);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            (await service.GetAllAsync()).Data!.TotalCount.Should().Be(1);
        }

        // EnableCategoria no pasa por ningún business validator antes de Activar(): el
        // caso "ya está activa" sale como excepción de dominio y no como Failure. Mismo
        // comportamiento que EnableProveedor; el test deja constancia del actual.
        [Fact]
        public async Task EnableCategoria_SobreCategoriaYaActiva_LanzaEstadoInvalidoException()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var service = IntegrationTestFactory.CrearCategoriaServices(context);

            var accion = async () => await service.EnableCategoria(categoria.Id);

            await accion.Should().ThrowAsync<EstadoInvalidoException>();
        }

        [Fact]
        public async Task DeleteAsync_SobreCategoriaExistente_LaBorraFisicamente()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var categoria = await SembrarCategoriaAsync(context);
            var service = IntegrationTestFactory.CrearCategoriaServices(context);

            var resultado = await service.DeleteAsync(categoria.Id);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            (await context.Categorias.CountAsync()).Should().Be(0);
        }

        [Theory]
        [InlineData(0, 10)]
        [InlineData(1, 0)]
        [InlineData(1, 101)]
        public async Task GetAllAsync_ConPaginacionFueraDeRango_DevuelveFailure(int pageNumber, int pageSize)
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearCategoriaServices(context);

            var resultado = await service.GetAllAsync(pageNumber, pageSize);

            resultado.IsSuccess.Should().BeFalse();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public async Task GetByIdAsync_ConIdInvalido_DevuelveFailure(int id)
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearCategoriaServices(context);

            var resultado = await service.GetByIdAsync(id);

            resultado.IsSuccess.Should().BeFalse();
        }
    }
}
