using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using ProductApp.Aplication.Dtos.UsuarioDto;
using ProductApp.Aplication.Helper;
using ProductApp.Domian.Common.Enums.EnumsUsuario;
using ProductApp.Domian.Entitis;
using Xunit;

namespace ProductApp.Tests.Integration
{
    // Cubre el CRUD administrativo de usuarios: alta, edición y listado paginado. Los otros
    // archivos de UsuarioService cubren contraseñas, perfil propio y documentos legales.
    //
    // Los formatos de los DTOs vienen de CreateUsuarioValidator/UpdateUsuarioValidator:
    // el username solo acepta letras y números (^[a-zA-Z0-9]+$), por eso acá es "aperez" y
    // no "usuario.test" como en los seeders que entran por la entidad y se saltan el validador.
    public class UsuarioServiceCrudTests
    {
        private static async Task<Usuario> SembrarUsuarioAsync(
            ProductApp.Infraesctructura.Persistencia.Contex.AppDbContext context,
            string nombre = "Ana Pérez",
            string email = "ana@test.com",
            string username = "aperez",
            RolUsuario rol = RolUsuario.Vendedor)
        {
            var usuario = new Usuario(nombre, email, username, rol);
            context.Usuarios.Add(usuario);
            await context.SaveChangesAsync();
            return usuario;
        }

        private static CreateUsuarioDto CrearDto(
            string nombre = "Ana Pérez",
            string email = "ana@test.com",
            string username = "aperez",
            string rol = nameof(RolUsuario.Vendedor),
            DateTime? fechaNacimiento = null)
            => new()
            {
                Nombre = nombre,
                Email = email,
                UserName = username,
                Password = "Passw0rd",
                RolUsuario = rol,
                FechaNacimiento = fechaNacimiento
            };

        private static UpdateUsuarioDto ActualizarDto(
            int id,
            string nombre = "Ana Pérez",
            string email = "ana@test.com",
            string username = "aperez",
            string rol = nameof(RolUsuario.Vendedor),
            DateTime? fechaNacimiento = null)
            => new()
            {
                Id = id,
                Nombre = nombre,
                Email = email,
                UserName = username,
                RolUsuario = rol,
                FechaNacimiento = fechaNacimiento
            };

        // --- CreateAsync ---

        [Fact]
        public async Task CreateAsync_ConDatosValidos_GuardaElUsuarioActivoConContraseñaTemporal()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearUsuarioService(context);
            // Relativa a hoy para que la edad esperada no caduque con el año.
            var fechaNacimiento = DateTime.UtcNow.AddYears(-30).Date;

            var resultado = await service.CreateAsync(CrearDto(fechaNacimiento: fechaNacimiento));

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.Email.Should().Be("ana@test.com");
            resultado.Data.UserName.Should().Be("aperez");
            resultado.Data.RolUsuario.Should().Be(nameof(RolUsuario.Vendedor));
            resultado.Data.EstadoUsuario.Should().Be(nameof(EstadoUsuario.Activo));
            // EstablecerFechaNacimiento lo hace el servicio, no el mapper: si se cayera, Edad sería null.
            resultado.Data.FechaNacimiento.Should().Be(fechaNacimiento);
            resultado.Data.Edad.Should().Be(30);

            (await context.Usuarios.CountAsync()).Should().Be(1);
            var guardado = await context.Usuarios.SingleAsync();
            // Todo usuario nuevo nace con la contraseña marcada como temporal: es lo que
            // RequiereCambioPasswordFilter usa para bloquear la API hasta que la cambie.
            guardado.DebeCambiarPassword.Should().BeTrue();
            PasswordHelper.Verify("Passw0rd", guardado.PasswordHash).Should().BeTrue();
        }

        [Fact]
        public async Task CreateAsync_ConElEmailDeUnUsuarioExistente_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            // Username distinto a propósito: la única regla que puede saltar es la del email.
            await SembrarUsuarioAsync(context, nombre: "Luis Gómez", email: "ana@test.com", username: "lgomez");
            var service = IntegrationTestFactory.CrearUsuarioService(context);

            var resultado = await service.CreateAsync(CrearDto(email: "ana@test.com", username: "aperez"));

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("El email o nombre de usuario ya está en uso");
            (await context.Usuarios.CountAsync()).Should().Be(1);
        }

        [Fact]
        public async Task CreateAsync_ConElNombreDeUsuarioDeUnUsuarioExistente_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            // Email distinto: acá la que salta es la otra mitad del OR del validador de negocio.
            await SembrarUsuarioAsync(context, nombre: "Luis Gómez", email: "luis@test.com", username: "aperez");
            var service = IntegrationTestFactory.CrearUsuarioService(context);

            var resultado = await service.CreateAsync(CrearDto(email: "ana@test.com", username: "aperez"));

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("El email o nombre de usuario ya está en uso");
            (await context.Usuarios.CountAsync()).Should().Be(1);
        }

        // Tiene que cortar el validador de forma: UsuarioMapper.MapToEntity hace Enum.Parse
        // sin TryParse, así que un rol inválido que llegara al mapper sería un 500, no un 400.
        [Fact]
        public async Task CreateAsync_ConUnRolQueNoExiste_DevuelveFailureYNoGuardaNada()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearUsuarioService(context);

            var resultado = await service.CreateAsync(CrearDto(rol: "Supervisor"));

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("El rol debe ser uno de");
            (await context.Usuarios.CountAsync()).Should().Be(0);
        }

        // --- UpdateAsync ---

        [Fact]
        public async Task UpdateAsync_ConDatosValidos_ActualizaNombreEmailUserNameRolYFechaNacimiento()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var usuario = await SembrarUsuarioAsync(context);
            var service = IntegrationTestFactory.CrearUsuarioService(context);
            var fechaNacimiento = DateTime.UtcNow.AddYears(-25).Date;

            var resultado = await service.UpdateAsync(ActualizarDto(
                usuario.Id,
                nombre: "Ana María Pérez",
                email: "ana.perez@test.com",
                username: "amperez",
                rol: nameof(RolUsuario.Administrador),
                fechaNacimiento: fechaNacimiento));

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.Nombre.Should().Be("Ana María Pérez");

            var guardado = await context.Usuarios.SingleAsync();
            guardado.Nombre.Should().Be("Ana María Pérez");
            guardado.Email.Should().Be("ana.perez@test.com");
            // Regresión: UsuarioMapper.MapUpdate validaba y deduplicaba el UserName pero nunca
            // lo aplicaba a la entidad — editar el nombre de usuario era un no-op silencioso.
            guardado.Username.Should().Be("amperez");
            guardado.RolUsuario.Should().Be(RolUsuario.Administrador);
            guardado.FechaNacimiento.Should().Be(fechaNacimiento);
        }

        [Fact]
        public async Task UpdateAsync_ConUsuarioInexistente_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearUsuarioService(context);

            var resultado = await service.UpdateAsync(ActualizarDto(999));

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("Usuario no encontrado");
        }

        // La baja de usuario es lógica: la fila sigue en la tabla, pero para la aplicación el
        // usuario ya no existe y no se puede editar.
        [Fact]
        public async Task UpdateAsync_ConUsuarioEliminado_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var usuario = await SembrarUsuarioAsync(context);
            usuario.Eliminar();
            await context.SaveChangesAsync();
            var service = IntegrationTestFactory.CrearUsuarioService(context);

            var resultado = await service.UpdateAsync(ActualizarDto(usuario.Id, nombre: "Nombre Cambiado"));

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("Usuario no encontrado");
        }

        [Fact]
        public async Task UpdateAsync_ConElEmailDeOtroUsuario_DevuelveFailureSinTocarLaEntidad()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            await SembrarUsuarioAsync(context, nombre: "Luis Gómez", email: "ocupado@test.com", username: "lgomez");
            var usuario = await SembrarUsuarioAsync(context);
            var service = IntegrationTestFactory.CrearUsuarioService(context);

            var resultado = await service.UpdateAsync(ActualizarDto(usuario.Id, email: "ocupado@test.com"));

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("ya está en uso por otro usuario");
            usuario.Email.Should().Be("ana@test.com");
        }

        [Fact]
        public async Task UpdateAsync_ConElNombreDeUsuarioDeOtroUsuario_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            await SembrarUsuarioAsync(context, nombre: "Luis Gómez", email: "luis@test.com", username: "lgomez");
            var usuario = await SembrarUsuarioAsync(context);
            var service = IntegrationTestFactory.CrearUsuarioService(context);

            var resultado = await service.UpdateAsync(ActualizarDto(usuario.Id, username: "lgomez"));

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("ya está en uso por otro usuario");
        }

        // El validador de negocio se excluye a sí mismo (x.Id != dto.Id): si no lo hiciera,
        // nadie podría corregir su nombre conservando su propio email y su propio username.
        [Fact]
        public async Task UpdateAsync_ConservandoSuPropioEmailYNombreDeUsuario_ActualizaElUsuario()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var usuario = await SembrarUsuarioAsync(context);
            var service = IntegrationTestFactory.CrearUsuarioService(context);

            var resultado = await service.UpdateAsync(ActualizarDto(
                usuario.Id, nombre: "Ana P. Gómez", email: usuario.Email, username: usuario.Username));

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.Nombre.Should().Be("Ana P. Gómez");
        }

        // El validador de forma corre antes de buscar al usuario: con un Id inválido el mensaje
        // es el del validador, no "Usuario no encontrado".
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task UpdateAsync_ConIdInvalido_DevuelveFailureDelValidador(int id)
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearUsuarioService(context);

            var resultado = await service.UpdateAsync(ActualizarDto(id));

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("El Id debe ser mayor que cero");
        }

        // Mismo motivo que en CreateAsync: UsuarioMapper.MapUpdate hace Enum.Parse sin TryParse.
        [Fact]
        public async Task UpdateAsync_ConUnRolQueNoExiste_DevuelveFailureDelValidador()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var usuario = await SembrarUsuarioAsync(context);
            var service = IntegrationTestFactory.CrearUsuarioService(context);

            var resultado = await service.UpdateAsync(ActualizarDto(usuario.Id, rol: "Supervisor"));

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("El rol debe ser uno de");
            usuario.RolUsuario.Should().Be(RolUsuario.Vendedor);
        }

        // Regresión: ValidarUpdateUsuarioAsync no protegía al último administrador activo pese
        // a que UsuarioMapper.MapUpdate también cambia el rol — la misma mutación que CambiarRol
        // ya bloqueaba por su propia puerta quedaba abierta por esta otra.
        [Fact]
        public async Task UpdateAsync_QuitandoleElRolAlUnicoAdministradorActivo_DevuelveFailure()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var administrador = await SembrarUsuarioAsync(context, rol: RolUsuario.Administrador);
            var service = IntegrationTestFactory.CrearUsuarioService(context);

            var resultado = await service.UpdateAsync(ActualizarDto(
                administrador.Id, rol: nameof(RolUsuario.Vendedor)));

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("último administrador activo");
            administrador.RolUsuario.Should().Be(RolUsuario.Administrador);
        }

        // --- GetAllAsync ---

        [Theory]
        [InlineData(0, 10)]
        [InlineData(1, 0)]
        [InlineData(1, 101)]
        public async Task GetAllAsync_ConPaginacionFueraDeRango_DevuelveFailure(int pageNumber, int pageSize)
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearUsuarioService(context);

            var resultado = await service.GetAllAsync(pageNumber, pageSize);

            resultado.IsSuccess.Should().BeFalse();
        }

        [Fact]
        public async Task GetAllAsync_SinUsuarios_DevuelveLaPaginaVacia()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var service = IntegrationTestFactory.CrearUsuarioService(context);

            var resultado = await service.GetAllAsync();

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.Items.Should().BeEmpty();
            resultado.Data.TotalCount.Should().Be(0);
            resultado.Data.TotalPages.Should().Be(0);
        }

        [Fact]
        public async Task GetAllAsync_PorDefecto_OmiteALosUsuariosInactivos()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var activo = await SembrarUsuarioAsync(context);
            var inactivo = await SembrarUsuarioAsync(
                context, nombre: "Luis Gómez", email: "luis@test.com", username: "lgomez");
            inactivo.Desactivar();
            await context.SaveChangesAsync();
            var service = IntegrationTestFactory.CrearUsuarioService(context);

            var soloActivos = await service.GetAllAsync();
            var conInactivos = await service.GetAllAsync(incluirInactivos: true);

            soloActivos.IsSuccess.Should().BeTrue(soloActivos.Message);
            soloActivos.Data!.TotalCount.Should().Be(1);
            soloActivos.Data.Items.Should().ContainSingle(u => u.Id == activo.Id);
            conInactivos.Data!.TotalCount.Should().Be(2);
        }

        // incluirInactivos abre la mano con los desactivados, no con los dados de baja:
        // un usuario eliminado no vuelve al listado por ninguna vía.
        [Fact]
        public async Task GetAllAsync_ConIncluirInactivos_SigueOmitiendoALosUsuariosEliminados()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            var usuario = await SembrarUsuarioAsync(context);
            usuario.Eliminar();
            await context.SaveChangesAsync();
            var service = IntegrationTestFactory.CrearUsuarioService(context);

            var resultado = await service.GetAllAsync(incluirInactivos: true);

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.TotalCount.Should().Be(0);
            resultado.Data.Items.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_ConMasUsuariosQueElTamañoDePagina_DevuelveSoloLaPaginaPedida()
        {
            using var context = IntegrationTestFactory.CrearContexto();
            await SembrarUsuarioAsync(context, nombre: "Uno", email: "uno@test.com", username: "uno");
            await SembrarUsuarioAsync(context, nombre: "Dos", email: "dos@test.com", username: "dos");
            await SembrarUsuarioAsync(context, nombre: "Tres", email: "tres@test.com", username: "tres");
            var service = IntegrationTestFactory.CrearUsuarioService(context);

            var primera = await service.GetAllAsync(pageNumber: 1, pageSize: 2);
            var segunda = await service.GetAllAsync(pageNumber: 2, pageSize: 2);

            primera.IsSuccess.Should().BeTrue(primera.Message);
            primera.Data!.Items.Should().HaveCount(2);
            primera.Data.TotalCount.Should().Be(3);
            primera.Data.TotalPages.Should().Be(2);
            primera.Data.PageNumber.Should().Be(1);
            primera.Data.PageSize.Should().Be(2);
            segunda.Data!.Items.Should().HaveCount(1);
            // GetAllUsuariosAsync ordena por Nombre: "Dos" y "Tres" van en la primera página,
            // "Uno" en la segunda — alfabético, no el orden en que se sembraron.
            primera.Data.Items.Select(u => u.Nombre).Should().Equal("Dos", "Tres");
            segunda.Data.Items.Select(u => u.Nombre).Should().Equal("Uno");
        }
    }
}
