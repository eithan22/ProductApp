using FluentAssertions;
using ProductApp.Aplication.Dtos.UsuarioDto;
using ProductApp.Aplication.Validators.Modulo_Usuario.UsuarioValidator;
using ProductApp.Domian.Common.Enums.EnumsUsuario;
using Xunit;

namespace ProductApp.Tests.Validators.Modulo_Usuario
{
    // Los tres validadores que aceptan un email escriben sobre la misma columna
    // nvarchar(50): los tres tienen que cortar en el mismo número.
    public class UsuarioValidatorTests
    {
        private static string CorreoDeLargo(int largo) => new string('a', largo - 9) + "@test.com";

        private static CreateUsuarioDto CrearDto() => new()
        {
            Nombre = "Ana Pérez",
            Email = "ana@test.com",
            Password = "Passw0rd",
            UserName = "aperez",
            RolUsuario = nameof(RolUsuario.Vendedor)
        };

        private static UpdateUsuarioDto ActualizarDto() => new()
        {
            Id = 1,
            Nombre = "Ana Pérez",
            Email = "ana@test.com",
            UserName = "aperez",
            RolUsuario = nameof(RolUsuario.Vendedor)
        };

        private static ActualizarMiPerfilDto PerfilDto() => new()
        {
            Nombre = "Ana Pérez",
            Email = "ana@test.com"
        };

        [Theory]
        [InlineData(50, true)]
        [InlineData(51, false)]
        public void CreateUsuario_ValidaElEmailContraElLargoDeLaColumna(int largo, bool esValido)
        {
            var dto = CrearDto();
            dto.Email = CorreoDeLargo(largo);

            new CreateUsuarioValidator().Validate(dto).IsValid.Should().Be(esValido);
        }

        [Theory]
        [InlineData(50, true)]
        [InlineData(51, false)]
        public void UpdateUsuario_ValidaElEmailContraElLargoDeLaColumna(int largo, bool esValido)
        {
            var dto = ActualizarDto();
            dto.Email = CorreoDeLargo(largo);

            new UpdateUsuarioValidator().Validate(dto).IsValid.Should().Be(esValido);
        }

        [Theory]
        [InlineData(50, true)]
        [InlineData(51, false)]
        public void ActualizarMiPerfil_ValidaElEmailContraElLargoDeLaColumna(int largo, bool esValido)
        {
            var dto = PerfilDto();
            dto.Email = CorreoDeLargo(largo);

            new ActualizarMiPerfilValidator().Validate(dto).IsValid.Should().Be(esValido);
        }

        // Los 100 caracteres que los tres validadores ya aceptaban solo son reales después
        // de la migración que amplió Usuarios.Nombre de nvarchar(20) a nvarchar(100).
        [Theory]
        [InlineData(100, true)]
        [InlineData(101, false)]
        public void CreateUsuario_ValidaElNombreContraElLargoDeLaColumna(int largo, bool esValido)
        {
            var dto = CrearDto();
            dto.Nombre = new string('a', largo);

            new CreateUsuarioValidator().Validate(dto).IsValid.Should().Be(esValido);
        }
    }
}
