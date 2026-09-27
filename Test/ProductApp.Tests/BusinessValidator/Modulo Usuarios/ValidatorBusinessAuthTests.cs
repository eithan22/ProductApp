using FluentAssertions;
using Moq;
using ProductApp.Aplication.BusinessValidator.Modulo_Usuarios;
using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto.AuthDto;
using ProductApp.Aplication.Helper;
using ProductApp.Domian.Common.Enums.EnumsUsuario;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace ProductApp.Tests.BusinessValidator.Modulo_Usuarios
{
    // Cubre ValidatorBusinessAuth.ValidarLoginAsync: la regla que decide si unas credenciales
    // son aceptables antes de que AuthServices emita el JWT. Incluye la regla de seguridad de
    // que TODOS los caminos de fallo (usuario inexistente, eliminado, inactivo/suspendido y
    // contraseña incorrecta) devuelvan el mismo mensaje genérico, y de que el motivo real
    // quede disponible aparte en MotivoInterno para el log del servidor.
    public class ValidatorBusinessAuthTests
    {
        private const string PasswordCorrecta = "Secreta@123";
        private const string MensajeGenerico = "Usuario o contraseña incorrectos";

        private static Usuario CrearUsuario(string username = "ana.torres", string password = PasswordCorrecta)
        {
            var usuario = new Usuario("Ana Torres", "ana@productapp.com", username, RolUsuario.Vendedor);
            usuario.EstablecerPasswordHash(PasswordHelper.Hash(password));
            return usuario;
        }

        private static (ValidatorBusinessAuth Validator, Mock<IUsuarioRepository> Repo) Crear(params Usuario[] usuarios)
        {
            var repo = new Mock<IUsuarioRepository>();

            repo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Usuario, bool>>>()))
                .Returns((Expression<Func<Usuario, bool>> filtro) =>
                    Task.FromResult(usuarios.FirstOrDefault(filtro.Compile())));

            return (new ValidatorBusinessAuth(repo.Object), repo);
        }

        private static LoginDto CrearDto(string username = "ana.torres", string password = PasswordCorrecta)
            => new() { Username = username, Password = password };

        [Fact]
        public async Task ValidarLoginAsync_ConCredencialesValidas_DevuelveSuccess()
        {
            var (validator, _) = Crear(CrearUsuario());

            var resultado = await validator.ValidarLoginAsync(CrearDto());

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
        }

        [Fact]
        public async Task ValidarLoginAsync_SinUsername_DevuelveFailureSinConsultarElRepositorio()
        {
            var (validator, repo) = Crear(CrearUsuario());

            var resultado = await validator.ValidarLoginAsync(CrearDto(username: string.Empty));

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be("El nombre de usuario y la contraseña son obligatorios");
            repo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ValidarLoginAsync_SinPassword_DevuelveFailureSinConsultarElRepositorio()
        {
            var (validator, repo) = Crear(CrearUsuario());

            var resultado = await validator.ValidarLoginAsync(CrearDto(password: string.Empty));

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be("El nombre de usuario y la contraseña son obligatorios");
            repo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ValidarLoginAsync_ConUsuarioInexistente_DevuelveFailureConMensajeGenerico()
        {
            var (validator, _) = Crear();

            var resultado = await validator.ValidarLoginAsync(CrearDto(username: "no.existe"));

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be(MensajeGenerico);
        }

        [Fact]
        public async Task ValidarLoginAsync_ConPasswordIncorrecta_DevuelveFailureConMensajeGenerico()
        {
            var (validator, _) = Crear(CrearUsuario());

            var resultado = await validator.ValidarLoginAsync(CrearDto(password: "Equivocada@1"));

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be(MensajeGenerico);
        }

        // Regla de seguridad (hallazgo M5): ninguna respuesta puede permitir distinguir si el
        // username existe. Los cuatro caminos de fallo tienen que ser indistinguibles entre sí.
        [Fact]
        public async Task ValidarLoginAsync_TodosLosCaminosDeFallo_DevuelvenExactamenteElMismoMensaje()
        {
            var (validator, _) = Crear(CrearUsuario());

            var usuarioInactivo = CrearUsuario();
            usuarioInactivo.Desactivar();
            var (validatorInactivo, _) = Crear(usuarioInactivo);

            var usuarioEliminado = CrearUsuario();
            usuarioEliminado.Eliminar();
            var (validatorEliminado, _) = Crear(usuarioEliminado);

            var usuarioInexistente = await validator.ValidarLoginAsync(CrearDto(username: "no.existe"));
            var passwordIncorrecta = await validator.ValidarLoginAsync(CrearDto(password: "Equivocada@1"));
            var inactivo = await validatorInactivo.ValidarLoginAsync(CrearDto());
            var eliminado = await validatorEliminado.ValidarLoginAsync(CrearDto());

            var resultados = new[] { usuarioInexistente, passwordIncorrecta, inactivo, eliminado };

            resultados.Should().OnlyContain(r => !r.IsSuccess);
            resultados.Select(r => r.Message).Should().AllBe(usuarioInexistente.Message);
        }

        [Fact]
        public async Task ValidarLoginAsync_ConUsuarioInactivo_DevuelveFailureConMensajeGenerico()
        {
            var usuario = CrearUsuario();
            usuario.Desactivar();
            var (validator, _) = Crear(usuario);

            var resultado = await validator.ValidarLoginAsync(CrearDto());

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be(MensajeGenerico);
            // El cliente no puede saber por qué falló, pero el log del servidor sí.
            resultado.MotivoInterno.Should().Be("El usuario está en estado Inactivo");
        }

        // El repositorio real (GenericRepository.FirstOrDefaultAsync) ya filtra EstaEliminado,
        // asi que en produccion un usuario eliminado llega como null y cae en el mensaje generico.
        // Este test fija el contrato de la rama por si el filtro del repositorio cambia.
        [Fact]
        public async Task ValidarLoginAsync_ConUsuarioEliminado_DevuelveFailureConMensajeGenerico()
        {
            var usuario = CrearUsuario();
            usuario.Eliminar();
            var (validator, _) = Crear(usuario);

            var resultado = await validator.ValidarLoginAsync(CrearDto());

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be(MensajeGenerico);
            resultado.MotivoInterno.Should().Be("El usuario está eliminado");
        }
    }
}
