using FluentAssertions;
using ProductApp.Aplication.Dtos.UsuarioDto;
using ProductApp.Domian.Common.Enums.EnumsUsuario;
using ProductApp.Domian.Entitis;
using Xunit;

namespace ProductApp.Tests.Integration
{
    // M10 (auditoría de seguridad): ActualizarMiPerfilAsync era el único método de
    // UsuarioService que se saltaba el validador de negocio (IValidatorBusinessUsuario).
    // El índice único UX_Usuarios_Email (M8) ya evitaba el duplicado a nivel de base, pero
    // sin este validador el usuario se enteraba con un error genérico de la base en vez de
    // un mensaje de negocio, y el método rompía el patrón Controller→Service→validators→Repository.
    public class UsuarioServicePerfilTests
    {
        private static async Task<Usuario> SembrarUsuarioAsync(
            Infraesctructura.Persistencia.Contex.AppDbContext context, string nombre, string email, string username)
        {
            var usuario = new Usuario(nombre, email, username, RolUsuario.Vendedor);
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();
            return usuario;
        }

        [Fact]
        public async Task ActualizarMiPerfilAsync_ConEmailDeOtroUsuario_DevuelveFailureSinTocarLaEntidad()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            await SembrarUsuarioAsync(context, "Otro Usuario", "ocupado@test.com", "otro.usuario");
            var usuario = await SembrarUsuarioAsync(context, "Usuario Test", "usuario@test.com", "usuario.test");
            var service = IntegrationTestFactory.CrearUsuarioService(context);

            var resultado = await service.ActualizarMiPerfilAsync(usuario.Id, new ActualizarMiPerfilDto
            {
                Nombre = "Usuario Test",
                Email = "ocupado@test.com"
            });

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("El email ya está en uso por otro usuario");
            usuario.Email.Should().Be("usuario@test.com");
        }

        [Fact]
        public async Task ActualizarMiPerfilAsync_ConservandoElPropioEmail_ActualizaCorrectamente()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var usuario = await SembrarUsuarioAsync(context, "Usuario Test", "usuario@test.com", "usuario.test");
            var service = IntegrationTestFactory.CrearUsuarioService(context);

            var resultado = await service.ActualizarMiPerfilAsync(usuario.Id, new ActualizarMiPerfilDto
            {
                Nombre = "Usuario Editado",
                Email = "usuario@test.com"
            });

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            usuario.Nombre.Should().Be("Usuario Editado");
        }

        [Fact]
        public async Task ActualizarMiPerfilAsync_ConEmailNuevoYLibre_ActualizaCorrectamente()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var usuario = await SembrarUsuarioAsync(context, "Usuario Test", "usuario@test.com", "usuario.test");
            var service = IntegrationTestFactory.CrearUsuarioService(context);

            var resultado = await service.ActualizarMiPerfilAsync(usuario.Id, new ActualizarMiPerfilDto
            {
                Nombre = "Usuario Test",
                Email = "nuevo@test.com"
            });

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            usuario.Email.Should().Be("nuevo@test.com");
        }
    }
}
