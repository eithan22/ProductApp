using FluentAssertions;
using Moq;
using ProductApp.Aplication.BusinessValidator.Modulo_Productos;
using ProductApp.Aplication.Dtos.Modulo_Productos.InventarioDto;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using Xunit;

namespace ProductApp.Tests.BusinessValidator.Modulo_Productos
{
    // Cubre ValidatorBusinessInventario.ValidarEntradaStockAsync: la regla de negocio que
    // decide si se puede sumar stock antes de que InventarioService toque el dominio.
    // Es la regla espejo de ValidarSalidaStockAsync, con una diferencia importante:
    // la entrada NO valida la cantidad del DTO (eso lo hace MovimientoStockValidator
    // con FluentValidation y, en última instancia, la propia entidad Inventario).
    public class ValidatorBusinessInventarioTests
    {
        private const int ProductoId = 7;

        private static (ValidatorBusinessInventario Validator, Mock<IProductoRepository> Repo) Crear(Producto? producto)
        {
            var repo = new Mock<IProductoRepository>();

            repo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(producto);

            return (new ValidatorBusinessInventario(repo.Object), repo);
        }

        private static Producto CrearProducto(bool activo = true)
        {
            var producto = new Producto("Teclado mecánico", "Switches azules", 2500m, 1200m, categoriaId: 1);

            if (!activo)
                producto.DesactivarProducto();

            return producto;
        }

        private static Inventario CrearInventario(int cantidadActual = 10, int cantidadMinima = 5)
            => new Inventario(cantidadActual, cantidadMinima, ProductoId);

        private static MovimientoStockDto CrearDto(int cantidad = 5)
            => new() { ProductoId = ProductoId, Cantidad = cantidad };

        [Fact]
        public async Task ValidarEntradaStockAsync_ConProductoActivo_DevuelveSuccess()
        {
            var (validator, _) = Crear(CrearProducto());

            var resultado = await validator.ValidarEntradaStockAsync(CrearDto(), CrearInventario());

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
        }

        [Fact]
        public async Task ValidarEntradaStockAsync_ConProductoInexistente_DevuelveFailure()
        {
            var (validator, _) = Crear(producto: null);

            var resultado = await validator.ValidarEntradaStockAsync(CrearDto(), CrearInventario());

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be("El producto asociado al inventario no existe.");
        }

        [Fact]
        public async Task ValidarEntradaStockAsync_ConProductoInactivo_DevuelveFailure()
        {
            var (validator, _) = Crear(CrearProducto(activo: false));

            var resultado = await validator.ValidarEntradaStockAsync(CrearDto(), CrearInventario());

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be("No se puede agregar stock a un producto inactivo.");
        }

        // El producto se busca por el ProductoId del INVENTARIO, no por el del DTO. Si algún día
        // se invierte, un DTO manipulado podría validarse contra otro producto.
        [Fact]
        public async Task ValidarEntradaStockAsync_BuscaElProductoPorElIdDelInventarioNoPorElDelDto()
        {
            var (validator, repo) = Crear(CrearProducto());
            var dto = new MovimientoStockDto { ProductoId = 999, Cantidad = 5 };

            await validator.ValidarEntradaStockAsync(dto, CrearInventario());

            repo.Verify(r => r.GetByIdAsync(ProductoId), Times.Once);
            repo.Verify(r => r.GetByIdAsync(999), Times.Never);
        }

        // Documenta el contrato real: a diferencia de la salida, la entrada no rechaza
        // cantidades inválidas. Esa defensa vive en MovimientoStockValidator y en la entidad.
        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public async Task ValidarEntradaStockAsync_NoValidaLaCantidadDelDto(int cantidad)
        {
            var (validator, _) = Crear(CrearProducto());

            var resultado = await validator.ValidarEntradaStockAsync(CrearDto(cantidad), CrearInventario());

            resultado.IsSuccess.Should().BeTrue();
        }

        // Contraste explícito con la salida, que sí corta por stock insuficiente.
        [Fact]
        public async Task ValidarSalidaStockAsync_ConCantidadMayorAlDisponible_DevuelveFailure()
        {
            var (validator, _) = Crear(CrearProducto());

            var resultado = await validator.ValidarSalidaStockAsync(CrearDto(cantidad: 50), CrearInventario(cantidadActual: 10));

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("Stock insuficiente");
        }

        [Fact]
        public async Task ValidarSalidaStockAsync_ConProductoInactivo_DevuelveFailureConMensajeDeSalida()
        {
            var (validator, _) = Crear(CrearProducto(activo: false));

            var resultado = await validator.ValidarSalidaStockAsync(CrearDto(), CrearInventario());

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be("No se puede descontar stock de un producto inactivo.");
        }
    }
}
