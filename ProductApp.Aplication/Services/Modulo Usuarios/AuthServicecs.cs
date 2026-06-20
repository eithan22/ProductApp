using FluentValidation;
using Microsoft.Extensions.Configuration;
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
    public class AuthServicecs : IAuthService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IMapperUsuario _mapperUsuario;
        private readonly IConfiguration _config;
        private readonly IValidator<LoginDto> _loginValidator;
        private readonly IValidator<RegisteDto> _registerValidator;
        private readonly IValidatorBusinessAuth _validatorBusinessAuth;

        public AuthServicecs(
            IUsuarioRepository usuarioRepository,
            IMapperUsuario mapperUsuario,
            IConfiguration config,
            IValidatorBusinessAuth validatorBusinessAuth,
            IValidator<LoginDto> loginValidator,
            IValidator<RegisteDto> registerValidator)
        {
            _usuarioRepository = usuarioRepository;
            _mapperUsuario = mapperUsuario;
            _config = config;
            _validatorBusinessAuth = validatorBusinessAuth;
            _loginValidator = loginValidator;
            _registerValidator = registerValidator;
        }

        public async Task<OperationResultD<UsuarioResponseDto>> Register(RegisteDto dto)
        {
            var validationResult = await _registerValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return OperationResultD<UsuarioResponseDto>.Failure(
                    string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

            var validatorBusiness = await _validatorBusinessAuth.ValidarRegisterAsync(dto);
            if (!validatorBusiness.IsSuccess)
                return OperationResultD<UsuarioResponseDto>.Failure(validatorBusiness.Message);

            var usuario = _mapperUsuario.MapFromRegisterDto(dto);
            usuario.EstablecerFechaNacimiento(dto.FechaNacimiento);
            usuario.EstablecerPasswordHash(PasswordHelper.Hash(dto.Password));

            await _usuarioRepository.CreateAsync(usuario);

            var response = _mapperUsuario.ToDto(usuario);
            return OperationResultD<UsuarioResponseDto>.Success(response, "Usuario registrado exitosamente");
        }

        public async Task<OperationResultD<AuthResponseDto>> Login(LoginDto dto)
        {
            var validationResult = await _loginValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
                return OperationResultD<AuthResponseDto>.Failure(
                    string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));

            var validatorBusiness = await _validatorBusinessAuth.ValidarLoginAsync(dto);
            if (!validatorBusiness.IsSuccess)
                return OperationResultD<AuthResponseDto>.Failure(validatorBusiness.Message);

            var usuario = await _usuarioRepository.FirstOrDefaultAsync(x => x.Username == dto.Username);
            if (usuario == null)
                return OperationResultD<AuthResponseDto>.Failure("Usuario no encontrado");

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Role, usuario.RolUsuario.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return OperationResultD<AuthResponseDto>.Success(new AuthResponseDto
            {
                Token = tokenString,
                Usuario = _mapperUsuario.ToDto(usuario)
            }, "Login exitoso");
        }
    }
}
