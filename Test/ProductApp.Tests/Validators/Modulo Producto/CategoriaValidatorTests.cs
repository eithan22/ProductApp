using FluentAssertions;
using ProductApp.Aplication.Dtos.CategoriaDto;
using ProductApp.Aplication.Validators.Modulo_Producto.CategoriaValidator;
using Xunit;

namespace ProductApp.Tests.Validators.Modulo_Producto
{
    // Create y Update escriben la misma columna nvarchar(100): Update aceptaba 200.
    public class CategoriaValidatorTests
    {
        [Theory]
        [InlineData(100, true)]
        [InlineData(101, false)]
        public void LosDosValidadoresDeCategoria_CortanLaDescripcionEnElMismoLargo(int largo, bool esValido)
        {
            var descripcion = new string('a', largo);

            var create = new CreateCategoriaValidator()
                .Validate(new CreateCategoriaDto { Nombre = "Ferretería", Descripcion = descripcion });
            var update = new UpdateCategoriaValidator()
                .Validate(new UpdateCategoriaDto { Id = 1, Nombre = "Ferretería", Descripcion = descripcion });

            create.IsValid.Should().Be(esValido);
            update.IsValid.Should().Be(esValido);
        }
    }
}
