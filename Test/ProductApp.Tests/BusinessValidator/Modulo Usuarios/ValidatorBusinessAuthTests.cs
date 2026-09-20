using FluentAssertions;
using Moq;
using ProductApp.Aplication.BusinessValidator.Modulo_Usuarios;
using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto.AuthDto;
using ProductApp.Aplication.Helper;
using ProductApp.Domian.Common.Enums.EnumsUsuario;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using System.Linq.Expressions;
using Xunit;

namespace ProductApp.Tests.BusinessValidator.Modulo_Usuarios
{
    // Cubre ValidatorBusinessAuth.ValidarLoginAsync: la regla que decide si unas credenciales
    // son aceptables antes de que AuthServices emita el JWT. Incluye la regla de seguridad de
    // que usuario inexistente y contraseña incorrecta devuelvan el mismo mensaje.
    public class ValidatorBusinessAuthTests
    {
        private const string PasswordCorrecta = "Secreta@123";

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
            resultado.Message.Should().Be("Usuario o contraseña incorrectos");
        }

        [Fact]
        public async Task ValidarLoginAsync_ConPasswordIncorrecta_DevuelveFailureConMensajeGenerico()
        {
            var (validator, _) = Crear(CrearUsuario());

            var resultado = await validator.ValidarLoginAsync(CrearDto(password: "Equivocada@1"));

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be("Usuario o contraseña incorrectos");
        }

        // Regla de seguridad: la respuesta no puede permitir distinguir si el usuario existe.
        [Fact]
        public async Task ValidarLoginAsync_UsuarioInexistenteYPasswordIncorrecta_DevuelvenExactamenteElMismoMensaje()
        {
            var (validator, _) = Crear(CrearUsuario());

            var usuarioInexistente = await validator.ValidarLoginAsync(CrearDto(username: "no.existe"));
            var passwordIncorrecta = await validator.ValidarLoginAsync(CrearDto(password: "Equivocada@1"));

            usuarioInexistente.IsSuccess.Should().BeFalse();
            passwordIncorrecta.IsSuccess.Should().BeFalse();
            passwordIncorrecta.Message.Should().Be(usuarioInexistente.Message);
        }

        [Fact]
        public async Task ValidarLoginAsync_ConUsuarioInactivo_DevuelveFailure()
        {
            var usuario = CrearUsuario();
            usuario.Desactivar();
            var (validator, _) = Crear(usuario);

            var resultado = await validator.ValidarLoginAsync(CrearDto());

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be("Usuario inactivo o suspendido");
        }

        // El repositorio real (GenericRepository.FirstOrDefaultAsync) ya filtra EstaEliminado,
        // asi que en produccion un usuario eliminado llega como null y cae en el mensaje generico.
        // Este test fija el contrato de la rama por si el filtro del repositorio cambia.
        [Fact]
        public async Task ValidarLoginAsync_ConUsuarioEliminado_DevuelveFailure()
        {
            var usuario = CrearUsuario();
            usuario.Eliminar();
            var (validator, _) = Crear(usuario);

            var resultado = await validator.ValidarLoginAsync(CrearDto());

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Be("Usuario deshabilitado");
        }
    }
}
