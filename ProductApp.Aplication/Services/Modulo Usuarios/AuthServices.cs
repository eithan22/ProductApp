using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto.AuthDto;
using ProductApp.Aplication.Dtos.UsuarioDto;
using ProductApp.Aplication.Helper;
using ProductApp.Aplication.Interface.IMappers.Modulo_Usuarios;
using ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Usuario;
using ProductApp.Aplication.Interface.Servicios.Modulo_Usuarios;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ProductApp.Aplication.Services.Modulo_Usuarios
{
    public class AuthServices : IAuthService
    {
        private const int DuracionTokenMinutosRespaldo = 60;

        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IConfiguracionSistemaRepository _configuracionSistemaRepository;
        private readonly IMapperUsuario _mapperUsuario;
        private readonly IConfiguration _config;
        private readonly IValidator<LoginDto> _loginValidator;
        private readonly IValidatorBusinessAuth _validatorBusinessAuth;
        private readonly ILogger<AuthServices> _logger;

        public AuthServices(
            IUsuarioRepository usuarioRepository,
            IConfiguracionSistemaRepository configuracionSistemaRepository,
            IMapperUsuario mapperUsuario,
            IConfiguration config,
            IValidatorBusinessAuth validatorBusinessAuth,
            IValidator<LoginDto> loginValidator,
            ILogger<AuthServices> logger)
        {
            _usuarioRepository = usuarioRepository;
            _configuracionSistemaRepository = configuracionSistemaRepository;
            _mapperUsuario = mapperUsuario;
            _config = config;
            _validatorBusinessAuth = validatorBusinessAuth;
            _loginValidator = loginValidator;
            _logger = logger;
        }

        public async Task<OperationResultD<AuthResponseDto>> Login(LoginDto dto)
        {
            var validationResult = await _loginValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return OperationResultD<AuthResponseDto>.Failure(
                    string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

            var validatorBusiness = await _validatorBusinessAuth.ValidarLoginAsync(dto);
            if (!validatorBusiness.IsSuccess)
            {
                _logger.LogWarning("Intento de login fallido para el usuario {Username} desde {Motivo}", dto.Username, validatorBusiness.Message);
                return OperationResultD<AuthResponseDto>.Failure(validatorBusiness.Message);
            }

            var usuario = await _usuarioRepository.FirstOrDefaultAsync(x => x.Username == dto.Username);
            if (usuario == null)
                return OperationResultD<AuthResponseDto>.Failure("Usuario no encontrado");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Role, usuario.RolUsuario.ToString()),
                new Claim("DebeCambiarPassword", usuario.DebeCambiarPassword.ToString())
            };

            var configuracion = await _configuracionSistemaRepository.ObtenerAsync();
            var duracionTokenMinutos = configuracion?.DuracionTokenMinutos ?? DuracionTokenMinutosRespaldo;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(duracionTokenMinutos),
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            _logger.LogInformation("Login exitoso para el usuario {Username} (Id {UsuarioId})", usuario.Username, usuario.Id);

            return OperationResultD<AuthResponseDto>.Success(new AuthResponseDto
            {
                Token = tokenString,
                DebeCambiarPassword = usuario.DebeCambiarPassword,
                Usuario = _mapperUsuario.ToDto(usuario)
            }, "Login exitoso");
        }
    }
}
