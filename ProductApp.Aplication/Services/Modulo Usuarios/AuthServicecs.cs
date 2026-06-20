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
using ProductApp.Domian.Common.Enums.EnumsUsuario;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Repository;
using System;
using System.Collections.Generic;
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

        public AuthServicecs(IUsuarioRepository usuarioRepository, 
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

            try
            {
                // 🟢 VALIDACIONES DE DTO
                var validationResult = await _registerValidator.ValidateAsync(dto);

                if (!validationResult.IsValid)
                {
                    return OperationResultD<UsuarioResponseDto>.Failure(string.Join("; ", validationResult.Errors));
                }



                // 🟢 VALIDACIONES DE NEGOCI

                var ValidatorBusiness = await _validatorBusinessAuth.ValidarRegisterAsync(dto);

                if (!ValidatorBusiness.IsSuccess)
                    return OperationResultD<UsuarioResponseDto>.Failure(ValidatorBusiness.Message);


                //  Crear nuevo usuario
                var usuario = _mapperUsuario.MapFromRegisterDto(dto);

                // Aquí deberías hashear la contraseña antes de guardarla
                usuario.EstablecerPasswordHash(PasswordHelper.Hash(dto.Password));

                //  Guardar usuario en la base de datos
                await _usuarioRepository.CreateAsync(usuario);

                // 🔴  Retornar resultado

                var response = _mapperUsuario.ToDto(usuario);

                return OperationResultD<UsuarioResponseDto>.Success(response, "Usuario registrado exitosamente");

            }
            catch (Exception ex)
            {
                return OperationResultD<UsuarioResponseDto>
                    .Failure(ex.Message);
            }



        }

        public async Task<OperationResultD<AuthResponseDto>> Login(LoginDto dto)
        {
            // 🟢 VALIDACIONES DE DTO
            var validationResult = await _loginValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                return OperationResultD<AuthResponseDto>.Failure(string.Join("; ", validationResult.Errors));
            }


             // 🟢 VALIDACIONES DE NEGOCIO
            var ValidatorBusiness = await _validatorBusinessAuth.ValidarLoginAsync(dto);

            if (!ValidatorBusiness.IsSuccess) 
                return OperationResultD<AuthResponseDto>.Failure(ValidatorBusiness.Message);


            var usuario = await _usuarioRepository
                .FirstOrDefaultAsync(x => x.Username == dto.Username);

            if (usuario == null)
                return OperationResultD<AuthResponseDto>.Failure("Usuario no encontrado");


            // 🟢 CREAR TOKEN JWT  // Aquí es donde se genera el token JWT para el usuario autenticado 

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nombre),
           new Claim(ClaimTypes.Role, usuario.RolUsuario.ToString())
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"])
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // 🟢 RESPUESTA

            return OperationResultD<AuthResponseDto>.Success(new AuthResponseDto
            {
                Token = tokenString,
                Usuario  = _mapperUsuario.ToDto(usuario)
            }, "Login exitoso");
        }


    }
}
