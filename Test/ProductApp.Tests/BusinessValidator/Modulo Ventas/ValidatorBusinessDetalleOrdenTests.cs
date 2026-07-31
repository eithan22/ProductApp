using FluentAssertions;
using Moq;
using ProductApp.Aplication.BusinessValidator.Modulo_Ventas;
using ProductApp.Aplication.Dtos.Modulo_Ventas.DetalleOrdenDto;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using Xunit;

namespace ProductApp.Tests.BusinessValidator.Modulo_Ventas
{
    public class ValidatorBusinessDetalleOrdenTests
    {
        private readonly Mock<IOrdenRepository> _ordenRepositoryMock = new();
        private readonly Mock<IDetalleOrdenRepository> _detalleOrdenRepositoryMock = new();
        private readonly Mock<IProductoRepository> _productoRepositoryMock = new();

        private ValidatorBusinessDetalleOrden CrearValidator()
            => new(_ordenRepositoryMock.Object, _detalleOrdenRepositoryMock.Object, _productoRepositoryMock.Object);

        private static Orden CrearOrdenPendiente() => new Orden(clienteId: 1, usuarioId: 1);

        private static Producto CrearProducto() => new Producto("Producto", "Descripcion", 10, 5, categoriaId: 1);

        private static void AsignarInventario(Producto producto, Inventario inventario)
        {
            typeof(Producto).GetProperty(nameof(Producto.Inventario))!.SetValue(producto, inventario);
        }

        // --- ValidarAgregarProductoAsync ---

        [Fact]
        public async Task ValidarAgregarProductoAsync_OrdenNoEncontrada_DevuelveFailure()
        {
            _ordenRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Orden?)null);
            var validator = CrearValidator();
            var dto = new CreateDetalleOrdenDto { OrdenId = 1, ProductId = 1, Cantidad = 1 };

            var resultado = await validator.ValidarAgregarProductoAsync(dto);

            resultado.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task ValidarAgregarProductoAsync_OrdenNoPendiente_DevuelveFailure()
        {
            var orden = CrearOrdenPendiente();
            orden.CancelarOrden();
            _ordenRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(orden);
            var validator = CrearValidator();
            var dto = new CreateDetalleOrdenDto { OrdenId = 1, ProductId = 1, Cantidad = 1 };

            var resultado = await validator.ValidarAgregarProductoAsync(dto);

            resultado.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task ValidarAgregarProductoAsync_ProductoNoEncontrado_DevuelveFailure()
        {
            var orden = CrearOrdenPendiente();
            _ordenRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(orden);
            _productoRepositoryMock.Setup(r => r.ObtenerConInventarioAsync(It.IsAny<int>())).ReturnsAsync((Producto?)null);
            var validator = CrearValidator();
            var dto = new CreateDetalleOrdenDto { OrdenId = 1, ProductId = 1, Cantidad = 1 };

            var resultado = await validator.ValidarAgregarProductoAsync(dto);

            resultado.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task ValidarAgregarProductoAsync_ProductoSinInventario_DevuelveFailure()
        {
            var orden = CrearOrdenPendiente();
            var producto = CrearProducto();
            _ordenRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(orden);
            _productoRepositoryMock.Setup(r => r.ObtenerConInventarioAsync(It.IsAny<int>())).ReturnsAsync(producto);
            var validator = CrearValidator();
            var dto = new CreateDetalleOrdenDto { OrdenId = 1, ProductId = 1, Cantidad = 1 };

            var resultado = await validator.ValidarAgregarProductoAsync(dto);

            resultado.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task ValidarAgregarProductoAsync_ProductoInactivo_DevuelveFailure()
        {
            var orden = CrearOrdenPendiente();
            var producto = CrearProducto();
            AsignarInventario(producto, new Inventario(cantidadActual: 10, cantidadMinima: 1, productoId: 1));
            producto.DesactivarProducto();
            _ordenRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(orden);
            _productoRepositoryMock.Setup(r => r.ObtenerConInventarioAsync(It.IsAny<int>())).ReturnsAsync(producto);
            var validator = CrearValidator();
            var dto = new CreateDetalleOrdenDto { OrdenId = 1, ProductId = 1, Cantidad = 1 };

            var resultado = await validator.ValidarAgregarProductoAsync(dto);

            resultado.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task ValidarAgregarProductoAsync_CantidadSumadaSuperaElStock_DevuelveFailure()
        {
            var orden = CrearOrdenPendiente();
            var producto = CrearProducto();
            AsignarInventario(producto, new Inventario(cantidadActual: 10, cantidadMinima: 1, productoId: 1));
            var detalleExistente = new OrdenDetalle(productId: 1, cantidad: 8, precioUnitario: 10, ordenId: 1);
            _ordenRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(orden);
            _productoRepositoryMock.Setup(r => r.ObtenerConInventarioAsync(It.IsAny<int>())).ReturnsAsync(producto);
            _detalleOrdenRepositoryMock.Setup(r => r.ObtenerProductoEnOrdenAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(detalleExistente);
            var validator = CrearValidator();
            var dto = new CreateDetalleOrdenDto { OrdenId = 1, ProductId = 1, Cantidad = 5 };

            var resultado = await validator.ValidarAgregarProductoAsync(dto);

            resultado.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task ValidarAgregarProductoAsync_CasoValido_DevuelveSuccess()
        {
            var orden = CrearOrdenPendiente();
            var producto = CrearProducto();
            AsignarInventario(producto, new Inventario(cantidadActual: 10, cantidadMinima: 1, productoId: 1));
            _ordenRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(orden);
            _productoRepositoryMock.Setup(r => r.ObtenerConInventarioAsync(It.IsAny<int>())).ReturnsAsync(producto);
            _detalleOrdenRepositoryMock.Setup(r => r.ObtenerProductoEnOrdenAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync((OrdenDetalle?)null);
            var validator = CrearValidator();
            var dto = new CreateDetalleOrdenDto { OrdenId = 1, ProductId = 1, Cantidad = 5 };

            var resultado = await validator.ValidarAgregarProductoAsync(dto);

            resultado.IsSuccess.Should().BeTrue();
        }

        // --- ValidarActualizarDetalleAsync ---

        [Fact]
        public async Task ValidarActualizarDetalleAsync_DetalleNoEncontrado_DevuelveFailure()
        {
            _detalleOrdenRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((OrdenDetalle?)null);
            var validator = CrearValidator();
            var dto = new UpdateDetalleOrdenDto { id = 1, Cantidad = 5 };

            var resultado = await validator.ValidarActualizarDetalleAsync(1, dto);

            resultado.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task ValidarActualizarDetalleAsync_OrdenNoPendiente_DevuelveFailure()
        {
            var detalle = new OrdenDetalle(productId: 1, cantidad: 2, precioUnitario: 10, ordenId: 1);
            var orden = CrearOrdenPendiente();
            orden.CancelarOrden();
            _detalleOrdenRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(detalle);
            _ordenRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(orden);
            var validator = CrearValidator();
            var dto = new UpdateDetalleOrdenDto { id = 1, Cantidad = 5 };

            var resultado = await validator.ValidarActualizarDetalleAsync(1, dto);

            resultado.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task ValidarActualizarDetalleAsync_CantidadSuperaElStock_DevuelveFailure()
        {
            var detalle = new OrdenDetalle(productId: 1, cantidad: 2, precioUnitario: 10, ordenId: 1);
            var orden = CrearOrdenPendiente();
            var producto = CrearProducto();
            AsignarInventario(producto, new Inventario(cantidadActual: 5, cantidadMinima: 1, productoId: 1));
            _detalleOrdenRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(detalle);
            _ordenRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(orden);
            _productoRepositoryMock.Setup(r => r.ObtenerConInventarioAsync(It.IsAny<int>())).ReturnsAsync(producto);
            var validator = CrearValidator();
            var dto = new UpdateDetalleOrdenDto { id = 1, Cantidad = 10 };

            var resultado = await validator.ValidarActualizarDetalleAsync(1, dto);

            resultado.IsSuccess.Should().BeFalse();
        }

        // --- ValidarEliminarDetalleAsync ---

        [Fact]
        public async Task ValidarEliminarDetalleAsync_DetalleNoEncontrado_DevuelveFailure()
        {
            _detalleOrdenRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((OrdenDetalle?)null);
            var validator = CrearValidator();

            var resultado = await validator.ValidarEliminarDetalleAsync(1);

            resultado.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task ValidarEliminarDetalleAsync_OrdenNoPendiente_DevuelveFailure()
        {
            var detalle = new OrdenDetalle(productId: 1, cantidad: 2, precioUnitario: 10, ordenId: 1);
            var orden = CrearOrdenPendiente();
            orden.CancelarOrden();
            _detalleOrdenRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(detalle);
            _ordenRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(orden);
            var validator = CrearValidator();

            var resultado = await validator.ValidarEliminarDetalleAsync(1);

            resultado.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task ValidarEliminarDetalleAsync_OrdenPendiente_DevuelveSuccess()
        {
            var detalle = new OrdenDetalle(productId: 1, cantidad: 2, precioUnitario: 10, ordenId: 1);
            var orden = CrearOrdenPendiente();
            _detalleOrdenRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(detalle);
            _ordenRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(orden);
            var validator = CrearValidator();

            var resultado = await validator.ValidarEliminarDetalleAsync(1);

            resultado.IsSuccess.Should().BeTrue();
        }
    }
}
