using FluentAssertions;
using ProductApp.Aplication.Dtos.ProductoDto;
using ProductApp.Aplication.Validators.Modulo_Producto.ProductoValidator;
using ProductApp.Domian.Entitis;
using Xunit;

namespace ProductApp.Tests.Validators.Modulo_Producto
{
    // El tope se expresa contra la constante del dominio, no contra el número literal:
    // el día que se mueva Producto.MontoMaximo, estos tests siguen midiendo lo mismo.
    public class ProductoValidatorTests
    {
        private static CreateProductoDto CrearDto()
            => new() { Nombre = "Martillo", Descripcion = "Descripción", Precio = 100m, Costo = 50m, CategoriaId = 1 };

        private static UpdateProductoDto ActualizarDto()
            => new() { Id = 1, Nombre = "Martillo", Descripcion = "Descripción", Precio = 100m, Costo = 50m, CategoriaId = 1 };

        [Fact]
        public void CreateProducto_ConElPrecioEnElTope_EsValido()
        {
            var dto = CrearDto();
            dto.Precio = Producto.MontoMaximo;

            new CreateProductoValidator().Validate(dto).IsValid.Should().BeTrue();
        }

        [Fact]
        public void CreateProducto_ConElPrecioSobreElTope_NoEsValido()
        {
            var dto = CrearDto();
            dto.Precio = Producto.MontoMaximo + 0.01m;

            var resultado = new CreateProductoValidator().Validate(dto);

            resultado.IsValid.Should().BeFalse();
            resultado.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProductoDto.Precio));
        }

        [Fact]
        public void CreateProducto_ConElCostoSobreElTope_NoEsValido()
        {
            var dto = CrearDto();
            dto.Costo = Producto.MontoMaximo + 0.01m;

            new CreateProductoValidator().Validate(dto).IsValid.Should().BeFalse();
        }

        [Fact]
        public void UpdateProducto_ConElPrecioSobreElTope_NoEsValido()
        {
            var dto = ActualizarDto();
            dto.Precio = Producto.MontoMaximo + 0.01m;

            new UpdateProductoValidator().Validate(dto).IsValid.Should().BeFalse();
        }

        [Fact]
        public void UpdateProducto_ConElCostoSobreElTope_NoEsValido()
        {
            var dto = ActualizarDto();
            dto.Costo = Producto.MontoMaximo + 0.01m;

            new UpdateProductoValidator().Validate(dto).IsValid.Should().BeFalse();
        }
    }
}
