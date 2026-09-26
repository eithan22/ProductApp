using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto.AuthDto;
using ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Usuario;
using ProductApp.Aplication.Mappers;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Aplication.Services.Modulo_Usuarios;
using ProductApp.Aplication.Validators.Modulo_Usuario.AuthValidator;
using ProductApp.Domian.Common.Base;
using ProductApp.Domian.Common.Enums.EnumsUsuario;
using ProductApp.Domian.Common.Legal;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;
using Xunit;

namespace ProductApp.Tests.Services.Modulo_Usuarios
{
    // Cubre AuthServices.Login: la puerta de entrada de todo el sistema. Verifica la emision
    // del JWT (claims y expiracion), que la duracion salga de ConfiguracionSistema con respaldo
    // de 60 minutos, y que un DTO invalido corte antes de tocar el repositorio.
    public class AuthServicesTests
    {
        private const string ClaveJwt = "clave-de-pruebas-unitarias-de-auth-con-mas-de-256-bits-0123456789";
        private const string EmisorJwt = "ProductApp.Tests";
        private const string AudienciaJwt = "ProductApp.Tests.Clientes";

        private static IConfiguration CrearConfiguration()
        {
            var config = new Mock<IConfiguration>();
            config.Setup(c => c["Jwt:Key"]).Returns(ClaveJwt);
            config.Setup(c => c["Jwt:Issuer"]).Returns(EmisorJwt);
            config.Setup(c => c["Jwt:Audience"]).Returns(AudienciaJwt);
            return config.Object;
        }

        private static (AuthServices Service,
                        Mock<IUsuarioRepository> UsuarioRepo,
                        Mock<IConfiguracionSistemaRepository> ConfiguracionRepo,
                        Mock<IValidatorBusinessAuth> ValidatorNegocio) Crear()
        {
            var usuarioRepo = new Mock<IUsuarioRepository>();
            var configuracionRepo = new Mock<IConfiguracionSistemaRepository>();
            var validatorNegocio = new Mock<IValidatorBusinessAuth>();

            validatorNegocio
                .Setup(v => v.ValidarLoginAsync(It.IsAny<LoginDto>()))
                .ReturnsAsync(OperationResult.Success());

            configuracionRepo
                .Setup(r => r.ObtenerAsync())
                .ReturnsAsync((ConfiguracionSistema?)null);

            var service = new AuthServices(
                usuarioRepo.Object,
                configuracionRepo.Object,
                new UsuarioMapper(),
                CrearConfiguration(),
                validatorNegocio.Object,
                new LoginValidator(),
                NullLogger<AuthServices>.Instance);

            return (service, usuarioRepo, configuracionRepo, validatorNegocio);
        }

        private static Usuario CrearUsuario(
            int id = 7,
            string username = "ana.torres",
            RolUsuario rol = RolUsuario.Administrador,
            bool debeCambiarPassword = false,
            string? versionDocumentosLegalesAceptada = null)
        {
            var usuario = new Usuario("Ana Torres", "ana@productapp.com", username, rol);

            if (debeCambiarPassword)
                usuario.MarcarPasswordComoTemporal();

            if (versionDocumentosLegalesAceptada != null)
                usuario.RegistrarAceptacionDocumentosLegales(versionDocumentosLegalesAceptada);

            // El Id lo asigna la base de datos; en un test unitario hay que forzarlo para poder
            // afirmar sobre el claim NameIdentifier. La propiedad se declara en BaseEntity, asi
            // que hay que pedirla desde ese tipo: desde Usuario el setter privado no es visible.
            typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!.SetValue(usuario, id);

            return usuario;
        }

        private static void DevolverUsuario(Mock<IUsuarioRepository> repo, Usuario? usuario)
            => repo.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Usuario, bool>>>()))
                   .ReturnsAsync(usuario);

        private static LoginDto CrearDto(string username = "ana.torres", string password = "Secreta@123")
            => new() { Username = username, Password = password };

        private static JwtSecurityToken Leer(string token)
            => new JwtSecurityTokenHandler().ReadJwtToken(token);

        [Fact]
        public async Task Login_ConCredencialesValidas_DevuelveTokenNoVacioYLosDatosDelUsuario()
        {
            var (service, usuarioRepo, _, _) = Crear();
            DevolverUsuario(usuarioRepo, CrearUsuario());

            var resultado = await service.Login(CrearDto());

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Message.Should().Be("Login exitoso");
            resultado.Data!.Token.Should().NotBeNullOrWhiteSpace();
            resultado.Data.Usuario.Id.Should().Be(7);
            resultado.Data.Usuario.UserName.Should().Be("ana.torres");
        }

        [Fact]
        public async Task Login_ConUsuarioQueTienePasswordTemporal_DevuelveDebeCambiarPasswordEnTrue()
        {
            var (service, usuarioRepo, _, _) = Crear();
            DevolverUsuario(usuarioRepo, CrearUsuario(debeCambiarPassword: true));

            var resultado = await service.Login(CrearDto());

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.DebeCambiarPassword.Should().BeTrue();
        }

        [Fact]
        public async Task Login_ConUsuarioSinCambioPendiente_DevuelveDebeCambiarPasswordEnFalse()
        {
            var (service, usuarioRepo, _, _) = Crear();
            DevolverUsuario(usuarioRepo, CrearUsuario(debeCambiarPassword: false));

            var resultado = await service.Login(CrearDto());

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.DebeCambiarPassword.Should().BeFalse();
        }

        // El flag existe solo para que la capa Web sepa si tiene que intercalar la pantalla de
        // aceptación justo después del login.
        [Fact]
        public async Task Login_ConUsuarioQueNoAceptoLosDocumentosLegales_DevuelveDebeAceptarEnTrue()
        {
            var (service, usuarioRepo, _, _) = Crear();
            DevolverUsuario(usuarioRepo, CrearUsuario());

            var resultado = await service.Login(CrearDto());

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.DebeAceptarDocumentosLegales.Should().BeTrue();
        }

        [Fact]
        public async Task Login_ConUsuarioQueYaAceptoLaVersionVigente_DevuelveDebeAceptarEnFalse()
        {
            var (service, usuarioRepo, _, _) = Crear();
            DevolverUsuario(usuarioRepo, CrearUsuario(
                versionDocumentosLegalesAceptada: DocumentosLegales.VersionVigente));

            var resultado = await service.Login(CrearDto());

            resultado.IsSuccess.Should().BeTrue(resultado.Message);
            resultado.Data!.DebeAceptarDocumentosLegales.Should().BeFalse();
        }

        // Decisión explícita, no olvido: la aceptación NO viaja como claim porque el estado cambia
        // dentro de la sesión y un claim obligaría a cerrarla para refrescarlo. El gate de la API
        // consulta la base. Si alguien agrega el claim, este test lo avisa.
        [Fact]
        public async Task Login_NoIncluyeLaAceptacionDeDocumentosLegalesComoClaim()
        {
            var (service, usuarioRepo, _, _) = Crear();
            DevolverUsuario(usuarioRepo, CrearUsuario());

            var resultado = await service.Login(CrearDto());

            var token = Leer(resultado.Data!.Token);

            token.Claims.Should().NotContain(c => c.Type == "DebeAceptarDocumentosLegales");
        }

        [Fact]
        public async Task Login_ConCredencialesValidas_EmiteUnTokenConLosClaimsDelUsuario()
        {
            var (service, usuarioRepo, _, _) = Crear();
            DevolverUsuario(usuarioRepo, CrearUsuario(id: 42, rol: RolUsuario.Vendedor, debeCambiarPassword: true));

            var resultado = await service.Login(CrearDto());

            var token = Leer(resultado.Data!.Token);

            token.Issuer.Should().Be(EmisorJwt);
            token.Audiences.Should().ContainSingle().Which.Should().Be(AudienciaJwt);
            token.Claims.Should().ContainSingle(c => c.Type == ClaimTypes.NameIdentifier).Which.Value.Should().Be("42");
            token.Claims.Should().ContainSingle(c => c.Type == ClaimTypes.Name).Which.Value.Should().Be("Ana Torres");
            token.Claims.Should().ContainSingle(c => c.Type == ClaimTypes.Role).Which.Value.Should().Be("Vendedor");
            token.Claims.Should().ContainSingle(c => c.Type == "DebeCambiarPassword").Which.Value.Should().Be("True");
        }

        [Fact]
        public async Task Login_ConConfiguracionCargada_UsaLaDuracionDeTokenConfigurada()
        {
            var (service, usuarioRepo, configuracionRepo, _) = Crear();
            DevolverUsuario(usuarioRepo, CrearUsuario());
            configuracionRepo
                .Setup(r => r.ObtenerAsync())
                .ReturnsAsync(new ConfiguracionSistema(
                    cantidadMinimaInventarioDefecto: 5,
                    duracionTokenMinutos: 15,
                    nombreEmpresa: "ProductApp",
                    moneda: "DOP"));

            var resultado = await service.Login(CrearDto());

            var token = Leer(resultado.Data!.Token);
            token.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(15), TimeSpan.FromSeconds(30));
        }

        [Fact]
        public async Task Login_SinConfiguracionCargada_UsaLaDuracionDeRespaldoDe60Minutos()
        {
            var (service, usuarioRepo, configuracionRepo, _) = Crear();
            DevolverUsuario(usuarioRepo, CrearUsuario());
            configuracionRepo.Setup(r => r.ObtenerAsync()).ReturnsAsync((ConfiguracionSistema?)null);

            var resultado = await service.Login(CrearDto());

            var token = Leer(resultado.Data!.Token);
            token.ValidTo.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(60), TimeSpan.FromSeconds(30));
        }

        [Fact]
        public async Task Login_ConUsernameVacio_FallaSinConsultarElRepositorioNiLasReglasDeNegocio()
        {
            var (service, usuarioRepo, _, validatorNegocio) = Crear();

            var resultado = await service.Login(CrearDto(username: string.Empty));

            resultado.IsSuccess.Should().BeFalse();
            resultado.Data.Should().BeNull();
            resultado.Message.Should().Contain("El nombre de usuario es requerido.");
            validatorNegocio.Verify(v => v.ValidarLoginAsync(It.IsAny<LoginDto>()), Times.Never);
            usuarioRepo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Login_ConPasswordVacia_FallaSinConsultarElRepositorioNiLasReglasDeNegocio()
        {
            var (service, usuarioRepo, _, validatorNegocio) = Crear();

            var resultado = await service.Login(CrearDto(password: string.Empty));

            resultado.IsSuccess.Should().BeFalse();
            resultado.Message.Should().Contain("La contraseña es requerida.");
            validatorNegocio.Verify(v => v.ValidarLoginAsync(It.IsAny<LoginDto>()), Times.Never);
            usuarioRepo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Login_CuandoLasReglasDeNegocioRechazan_DevuelveEseMensajeYNoEmiteToken()
        {
            var (service, usuarioRepo, _, validatorNegocio) = Crear();
            validatorNegocio
                .Setup(v => v.ValidarLoginAsync(It.IsAny<LoginDto>()))
                .ReturnsAsync(OperationResult.Failure("Usuario o contraseña incorrectos"));

            var resultado = await service.Login(CrearDto());

            resultado.IsSuccess.Should().BeFalse();
            resultado.Data.Should().BeNull();
            resultado.Message.Should().Be("Usuario o contraseña incorrectos");
            usuarioRepo.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Login_CuandoElUsuarioNoAparece_DevuelveElMensajeGenericoYNoEmiteToken()
        {
            var (service, usuarioRepo, _, _) = Crear();
            DevolverUsuario(usuarioRepo, null);

            var resultado = await service.Login(CrearDto());

            resultado.IsSuccess.Should().BeFalse();
            resultado.Data.Should().BeNull();
            resultado.Message.Should().Be("Usuario o contraseña incorrectos");
        }
    }
}
