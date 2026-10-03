using FluentAssertions;
using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto;
using ProductApp.Domian.Common.Enums.EnumsUsuario;
using ProductApp.Domian.Entitis;
using Xunit;

namespace ProductApp.Tests.Integration
{
    // Cubre el cambio de rol y, sobre todo, la regla que protege al sistema de quedarse sin
    // ningún administrador activo: si eso pasara, nadie podría volver a gestionar usuarios,
    // roles ni configuración.
    public class UsuarioServiceCambiarRolTests
    {
        private static async Task<Usuario> SembrarUsuarioAsync(
            ProductApp.Infraesctructura.Persistencia.Contex.AppDbContext context,
            RolUsuario rol = RolUsuario.Vendedor,
            string nombre = "Ana Pérez",
            string email = "ana@test.com",
            string username = "aperez")
        {
            var usuario = new Usuario(nombre, email, username, rol);
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();
            return usuario;
        }

        [Fact]
        public async Task CambiarRol_DeVendedorAAdministrador_ActualizaElRol()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var usuario = await SembrarUsuarioAsync(context);
            var service = IntegrationTestFactory.CrearUsuarioService(context);

            var resultado = await service.CambiarRol(
                new CambiarRolDto { Id = usuario.Id, NuevoRol = nameof(RolUsuario.Administrador) },
                usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data.Should().BeTrue();
            usuario.RolUsuario.Should().Be(RolUsuario.Administrador);
        }

        // El rol llega como texto desde el cliente: Enum.Parse(..., true) no depende de la caja,
        // mismo criterio que VerificadorSesionService con el claim del token.
        [Fact]
        public async Task CambiarRol_ConElRolEnOtraCaja_ActualizaElRol()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var usuario = await SembrarUsuarioAsync(context);
            var service = IntegrationTestFactory.CrearUsuarioService(context);

            var resultado = await service.CambiarRol(
                new CambiarRolDto { Id = usuario.Id, NuevoRol = "administrador" },
                usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            usuario.RolUsuario.Should().Be(RolUsuario.Administrador);
        }

        [Fact]
        public async Task CambiarRol_ConUsuarioInexistente_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearUsuarioService(context);

            var resultado = await service.CambiarRol(
                new CambiarRolDto { Id = 999, NuevoRol = nameof(RolUsuario.Administrador) },
                usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("Usuario no encontrado");
        }

        // Igual que en el CRUD: el servicio hace Enum.Parse sin TryParse después del validador,
        // así que un rol inexistente que se colara sería un 500.
        [Fact]
        public async Task CambiarRol_ConUnRolQueNoExiste_DevuelveFailureDelValidador()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var usuario = await SembrarUsuarioAsync(context);
            var service = IntegrationTestFactory.CrearUsuarioService(context);

            var resultado = await service.CambiarRol(
                new CambiarRolDto { Id = usuario.Id, NuevoRol = "Supervisor" },
                usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("El rol debe ser uno de");
            usuario.RolUsuario.Should().Be(RolUsuario.Vendedor);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task CambiarRol_ConIdInvalido_DevuelveFailureDelValidador(int id)
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearUsuarioService(context);

            var resultado = await service.CambiarRol(
                new CambiarRolDto { Id = id, NuevoRol = nameof(RolUsuario.Vendedor) },
                usuarioSolicitanteId: 1);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("El Id del usuario debe ser mayor que cero");
        }

        // El caso que la regla existe para evitar: degradar al único administrador activo
        // dejaría el sistema sin nadie que pueda administrarlo.
        [Fact]
        public async Task CambiarRol_QuitandoleElRolAlUnicoAdministradorActivo_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var administrador = await SembrarUsuarioAsync(context, RolUsuario.Administrador);
            var service = IntegrationTestFactory.CrearUsuarioService(context);

            var resultado = await service.CambiarRol(
                new CambiarRolDto { Id = administrador.Id, NuevoRol = nameof(RolUsuario.Vendedor) },
                usuarioSolicitanteId: administrador.Id);

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("es el último administrador activo del sistema");
            administrador.RolUsuario.Should().Be(RolUsuario.Administrador);
        }

        [Fact]
        public async Task CambiarRol_QuitandoleElRolAUnAdministradorHabiendoOtroActivo_ActualizaElRol()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var primero = await SembrarUsuarioAsync(context, RolUsuario.Administrador);
            var segundo = await SembrarUsuarioAsync(
                context, RolUsuario.Administrador,
                nombre: "Luis Gómez", email: "luis@test.com", username: "lgomez");
            var service = IntegrationTestFactory.CrearUsuarioService(context);

            var resultado = await service.CambiarRol(
                new CambiarRolDto { Id = segundo.Id, NuevoRol = nameof(RolUsuario.Vendedor) },
                usuarioSolicitanteId: primero.Id);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            segundo.RolUsuario.Should().Be(RolUsuario.Vendedor);
            primero.RolUsuario.Should().Be(RolUsuario.Administrador);
        }

        // La regla bloquea quitar el rol, no reenviarlo: un doble clic sobre "Administrador"
        // en el único administrador no debe dar error.
        [Fact]
        public async Task CambiarRol_AlUnicoAdministradorConservandoSuMismoRol_DevuelveSuccess()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var administrador = await SembrarUsuarioAsync(context, RolUsuario.Administrador);
            var service = IntegrationTestFactory.CrearUsuarioService(context);

            var resultado = await service.CambiarRol(
                new CambiarRolDto { Id = administrador.Id, NuevoRol = nameof(RolUsuario.Administrador) },
                usuarioSolicitanteId: administrador.Id);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            administrador.RolUsuario.Should().Be(RolUsuario.Administrador);
        }
    }
}
