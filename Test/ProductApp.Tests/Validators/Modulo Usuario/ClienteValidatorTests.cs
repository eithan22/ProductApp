using FluentAssertions;
using ProductApp.Aplication.Dtos.ClienteDto;
using ProductApp.Aplication.Validators.Modulo_Usuario.ClienteValidator;
using Xunit;

namespace ProductApp.Tests.Validators.Modulo_Usuario
{
    // Ninguno de estos casos toca la base a propósito. Cada valor de más es un largo que
    // la columna de SQL Server no aguanta: si el validador lo deja pasar, el error aparece
    // recién en SaveChanges y sale como 500 genérico. Acá se fija que muera antes, como
    // Failure de validación. EF InMemory no aplica HasMaxLength, así que esto no se puede
    // cubrir desde los tests de integración.
    public class ClienteValidatorTests
    {
        private static CreateClienteDto CrearDto() => new()
        {
            Nombre = "Ana Pérez",
            Cedula = "40212345678",
            Correo = "ana@test.com",
            Telefono = "8095551234",
            Direccion = "Calle Duarte 10"
        };

        private static UpdateClienteDto ActualizarDto() => new()
        {
            Id = 1,
            Nombre = "Ana Pérez",
            Cedula = "40212345678",
            Correo = "ana@test.com",
            Telefono = "8095551234",
            Direccion = "Calle Duarte 10"
        };

        // Arma un correo con formato válido y largo exacto: "@test.com" ocupa 9.
        private static string CorreoDeLargo(int largo) => new string('a', largo - 9) + "@test.com";

        [Theory]
        [InlineData(100, true)]   // largo exacto de la columna tras la ampliación
        [InlineData(101, false)]
        public void CreateCliente_ValidaElNombreContraElLargoDeLaColumna(int largo, bool esValido)
        {
            var dto = CrearDto();
            dto.Nombre = new string('a', largo);

            new CreateClienteValidator().Validate(dto).IsValid.Should().Be(esValido);
        }

        [Theory]
        [InlineData(100, true)]
        [InlineData(101, false)]
        public void UpdateCliente_ValidaLaDireccionContraElLargoDeLaColumna(int largo, bool esValido)
        {
            var dto = ActualizarDto();
            dto.Direccion = new string('a', largo);

            new UpdateClienteValidator().Validate(dto).IsValid.Should().Be(esValido);
        }

        [Theory]
        [InlineData(30, true)]
        [InlineData(31, false)]
        public void UpdateCliente_ValidaElCorreoContraElLargoDeLaColumna(int largo, bool esValido)
        {
            var dto = ActualizarDto();
            dto.Correo = CorreoDeLargo(largo);

            new UpdateClienteValidator().Validate(dto).IsValid.Should().Be(esValido);
        }

        // Los dos validadores tienen que pedir el MISMO formato de cédula: mientras Create
        // exigió 11 y Update 10, ningún cliente creado por la vía normal pudo editarse.
        [Theory]
        [InlineData("40212345678", true)]
        [InlineData("4021234567", false)]
        public void LosDosValidadoresDeCliente_ExigenElMismoFormatoDeCedula(string cedula, bool esValido)
        {
            var create = CrearDto();
            create.Cedula = cedula;
            var update = ActualizarDto();
            update.Cedula = cedula;

            new CreateClienteValidator().Validate(create).IsValid.Should().Be(esValido);
            new UpdateClienteValidator().Validate(update).IsValid.Should().Be(esValido);
        }
    }
}
