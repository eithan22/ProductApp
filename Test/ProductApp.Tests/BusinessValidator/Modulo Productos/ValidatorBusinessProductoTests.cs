using System.Linq.Expressions;
using FluentAssertions;
using Moq;
using ProductApp.Aplication.BusinessValidator.Modulo_Productos;
using ProductApp.Aplication.Dtos.ProductoDto;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using Xunit;

namespace ProductApp.Tests.BusinessValidator.Modulo_Productos
{
    // Cubre las tres reglas de negocio de Producto. La parte interesante no es el
    // Success/Failure (eso lo verifica también el test de integración del servicio),
    // sino el predicado exacto que se le manda al repositorio: en el update tiene que
    // excluir el propio Id, o editar un producto sin cambiarle el nombre se reportaría
    // como duplicado.
    public class ValidatorBusinessProductoTests
    {
        // Los dos catálogos referenciados (categoría y proveedor) se devuelven existentes por
        // defecto: así cada test sigue midiendo la regla de unicidad de nombre y no la de
        // referencias, que tiene sus propios casos más abajo.
        private static (ValidatorBusinessProducto Validator, Mock<IProductoRepository> Repo) Crear(bool existe)
        {
            var repo = new Mock<IProductoRepository>();

            repo.Setup(r => r.ExisteAsync(It.IsAny<Expression<Func<Producto, bool>>>()))
                .ReturnsAsync(existe);

            var categoriaRepo = new Mock<ICategoriaRepository>();
            categoriaRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(CrearCategoria());

            var proveedorRepo = new Mock<IProveedorRepository>();
            proveedorRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(CrearProveedor());

            return (new ValidatorBusinessProducto(repo.Object, categoriaRepo.Object, proveedorRepo.Object), repo);
        }

        private static Categoria CrearCategoria() => new("Ferretería", "Herramientas y tornillería");

        private static Proveedor CrearProveedor(bool activo = true)
        {
            var proveedor = new Proveedor("Proveedor Test", "8090000000", "proveedor@test.com", "Av. Principal 1");

            if (!activo)
                proveedor.Desactivar();

            return proveedor;
        }

        private static Producto CrearProducto(string nombre = "Martillo", bool activo = true)
        {
            var producto = new Producto(nombre, "Descripción", 100m, 50m, categoriaId: 1);

            if (!activo)
                producto.DesactivarProducto();

            return producto;
        }

        private static CreateProductoDto CreateDto(string nombre = "Martillo")
            => new() { Nombre = nombre, Descripcion = "Descripción", Precio = 100m, Costo = 50m, CategoriaId = 1 };

        private static UpdateProductoDto UpdateDto(int id, string nombre = "Martillo")
            => new() { Id = id, Nombre = nombre, Descripcion = "Descripción", Precio = 100m, Costo = 50m, CategoriaId = 1 };

        // Captura el predicado que el validador le pasa al repositorio para poder
        // evaluarlo sobre entidades concretas en vez de confiar en un mock que siempre
        // devuelve lo mismo.
        private static Expression<Func<Producto, bool>> CapturarPredicado(
            Mock<IProductoRepository> repo)
        {
            repo.Verify(r => r.ExisteAsync(It.IsAny<Expression<Func<Producto, bool>>>()), Times.Once);

            return (Expression<Func<Producto, bool>>)repo.Invocations
                .Single(i => i.Method.Name == nameof(IProductoRepository.ExisteAsync))
                .Arguments[0];
        }

        // ---------------------------------------------------------------
        // ValidarCreateProductoAsync
        // ---------------------------------------------------------------

        [Fact]
        public async Task ValidarCreateProductoAsync_SinNombreRepetido_DevuelveSuccess()
        {
            var (validator, _) = Crear(existe: false);

            var resultado = await validator.ValidarCreateProductoAsync(CreateDto());

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
        }

        [Fact]
        public async Task ValidarCreateProductoAsync_ConNombreRepetido_DevuelveFailure()
        {
            var (validator, _) = Crear(existe: true);

            var resultado = await validator.ValidarCreateProductoAsync(CreateDto());

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("Ya existe un producto con ese nombre");
        }

        // El create compara solo por nombre: no filtra por estado ni por categoría, así que
        // un producto inactivo con el mismo nombre también cuenta como duplicado.
        [Fact]
        public async Task ValidarCreateProductoAsync_ComparaSoloPorNombreSinMirarElEstado()
        {
            var (validator, repo) = Crear(existe: false);

            await validator.ValidarCreateProductoAsync(CreateDto("Martillo"));

            var predicado = CapturarPredicado(repo).Compile();
            predicado(CrearProducto("Martillo")).Should().BeTrue();
            predicado(CrearProducto("Martillo", activo: false)).Should().BeTrue();
            predicado(CrearProducto("Destornillador")).Should().BeFalse();
        }

        // ---------------------------------------------------------------
        // ValidarUpdateProductoAsync — el hueco que marcó la auditoría
        // ---------------------------------------------------------------

        [Fact]
        public async Task ValidarUpdateProductoAsync_SinOtroProductoConEseNombre_DevuelveSuccess()
        {
            var (validator, _) = Crear(existe: false);

            var resultado = await validator.ValidarUpdateProductoAsync(UpdateDto(id: 7), CrearProducto());

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
        }

        [Fact]
        public async Task ValidarUpdateProductoAsync_ConOtroProductoConEseNombre_DevuelveFailure()
        {
            var (validator, _) = Crear(existe: true);

            var resultado = await validator.ValidarUpdateProductoAsync(UpdateDto(id: 7), CrearProducto());

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("Ya existe otro producto con ese nombre");
        }

        // El producto que se está editando tiene Id 0 porque la entidad no deja asignarlo
        // (BaseEntity.Id es private set y lo pone EF). Eso alcanza para probar la exclusión:
        // con dto.Id = 0 el predicado se apaga sobre ese mismo producto, y con dto.Id = 7
        // se enciende sobre cualquier otro que tenga el nombre.
        [Fact]
        public async Task ValidarUpdateProductoAsync_ExcluyeAlPropioProductoDeLaBusquedaDeDuplicados()
        {
            var (validator, repo) = Crear(existe: false);
            var productoEditado = CrearProducto("Martillo");

            await validator.ValidarUpdateProductoAsync(UpdateDto(id: productoEditado.Id, nombre: "Martillo"), productoEditado);

            var predicado = CapturarPredicado(repo).Compile();
            predicado(productoEditado).Should().BeFalse("el producto que se está editando no puede ser su propio duplicado");
        }

        [Fact]
        public async Task ValidarUpdateProductoAsync_SiEsOtroProductoConElMismoNombre_ElPredicadoLoDetecta()
        {
            var (validator, repo) = Crear(existe: false);

            await validator.ValidarUpdateProductoAsync(UpdateDto(id: 7, nombre: "Martillo"), CrearProducto("Destornillador"));

            var predicado = CapturarPredicado(repo).Compile();
            predicado(CrearProducto("Martillo")).Should().BeTrue();
            predicado(CrearProducto("Taladro")).Should().BeFalse();
        }

        // ---------------------------------------------------------------
        // ValidarDisableProductoAsync
        // ---------------------------------------------------------------

        [Fact]
        public async Task ValidarDisableProductoAsync_ConProductoActivo_DevuelveSuccess()
        {
            var (validator, _) = Crear(existe: false);

            var resultado = await validator.ValidarDisableProductoAsync(CrearProducto());

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
        }

        [Fact]
        public async Task ValidarDisableProductoAsync_ConProductoYaInactivo_DevuelveFailure()
        {
            var (validator, _) = Crear(existe: false);

            var resultado = await validator.ValidarDisableProductoAsync(CrearProducto(activo: false));

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("ya está inactivo");
        }

        // No consulta la base: la regla se resuelve con el estado de la entidad que ya
        // trae el servicio.
        [Fact]
        public async Task ValidarDisableProductoAsync_NoConsultaAlRepositorio()
        {
            var (validator, repo) = Crear(existe: false);

            await validator.ValidarDisableProductoAsync(CrearProducto());

            repo.Verify(r => r.ExisteAsync(It.IsAny<Expression<Func<Producto, bool>>>()), Times.Never);
        }
    }
}
