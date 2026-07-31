using FluentAssertions;
using ProductApp.Domian.Common.Enums.EnumsProducto;
using ProductApp.Domian.Common.Exceptions;
using ProductApp.Domian.Common.Exceptions.ExceptionsProducto;
using ProductApp.Domian.Entitis;
using Xunit;

namespace ProductApp.Tests.Entitis
{
    public class ProductoTests
    {
        private static Producto CrearProducto(
            string nombre = "Producto",
            string descripcion = "Descripcion",
            decimal precio = 10,
            decimal costo = 5,
            int categoriaId = 1)
            => new Producto(nombre, descripcion, precio, costo, categoriaId);

        [Fact]
        public void Constructor_ConPrecioNegativo_LanzaPrecioInvalidoException()
        {
            var accion = () => CrearProducto(precio: -1);

            accion.Should().Throw<PrecioInvalidoException>();
        }

        [Fact]
        public void Constructor_ConCostoNegativo_LanzaValidacionDominioException()
        {
            var accion = () => CrearProducto(costo: -1);

            accion.Should().Throw<ValidacionDominioException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_ConNombreVacioOEnBlanco_LanzaValidacionDominioException(string nombre)
        {
            var accion = () => CrearProducto(nombre: nombre);

            accion.Should().Throw<ValidacionDominioException>();
        }

        [Fact]
        public void Constructor_ConCategoriaIdMenorOIgualACero_LanzaValidacionDominioException()
        {
            var accion = () => CrearProducto(categoriaId: 0);

            accion.Should().Throw<ValidacionDominioException>();
        }

        [Fact]
        public void DesactivarProducto_EnProductoActivo_LoDejaInactivo()
        {
            var producto = CrearProducto();

            producto.DesactivarProducto();

            producto.Estado.Should().Be(EstadoProducto.Inactivo);
        }

        [Fact]
        public void DesactivarProducto_EnProductoYaInactivo_LanzaEstadoInvalidoException()
        {
            var producto = CrearProducto();
            producto.DesactivarProducto();

            var accion = () => producto.DesactivarProducto();

            accion.Should().Throw<EstadoInvalidoException>();
        }

        [Fact]
        public void ActivarProducto_EnProductoInactivo_LoDejaActivo()
        {
            var producto = CrearProducto();
            producto.DesactivarProducto();

            producto.ActivarProducto();

            producto.Estado.Should().Be(EstadoProducto.Activo);
        }

        [Fact]
        public void ActivarProducto_EnProductoYaActivo_LanzaEstadoInvalidoException()
        {
            var producto = CrearProducto();

            var accion = () => producto.ActivarProducto();

            accion.Should().Throw<EstadoInvalidoException>();
        }
    }
}
