using FluentValidation;
using ProductApp.Aplication.BusinessValidator.Modulo_Usuarios;
using ProductApp.Aplication.Dtos.ClienteDto;
using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto;
using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto.AuthDto;
using ProductApp.Aplication.Dtos.UsuarioDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Interface.IMappers.Modulo_Usuarios;
using ProductApp.Aplication.Interface.RulesBusinnes;
using ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Usuario;
using ProductApp.Aplication.Interface.Servicios.Modulo_Usuarios;
using ProductApp.Aplication.Mappers;
using ProductApp.Aplication.Services;
using ProductApp.Aplication.Services.Modulo_Usuarios;
using ProductApp.Aplication.Validators.Modulo_Usuario.AuthValidator;
using ProductApp.Aplication.Validators.Modulo_Usuario.ClienteValidator;
using ProductApp.Aplication.Validators.Modulo_Usuario.UsuarioValidator;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Repository;

namespace ProductApp.Extensions.Modulo_Usuarios
{
    public static class UsuarioDependenciesExtension
    {
        public static IServiceCollection AddModuloUsuarios(this IServiceCollection services)
        {
            // Repositorios
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IClienteRepository, ClienteRepository>();

            // Mappers
            services.AddScoped<IMapperUsuario, UsuarioMapper>();
            services.AddScoped<IMapperCliente, ClienteMappers>();

            // Servicios
            services.AddScoped<IUsuarioService, UsuarioService>();
            services.AddScoped<IClienteServices, ClienteServices>();
            services.AddScoped<IAuthService, AuthServices>();

            // Reglas de negocio
            services.AddScoped<IValidatorBusinessUsuario, ValidatorBusinessUsuarios>();
            services.AddScoped<IValidatorBusinessAuth, ValidatorBusinessAuth>();
            services.AddScoped<IValidatorBusinessClientes, ValidatorBusinessClientes>();

            // Validadores DTO — Usuarios
            services.AddScoped<IValidator<CreateUsuarioDto>, CreateUsuarioValidator>();
            services.AddScoped<IValidator<UpdateUsuarioDto>, UpdateUsuarioValidator>();
            services.AddScoped<IValidator<ChangePasswordDto>, ChangePasswordUsuarioValidator>();
            services.AddScoped<IValidator<ResetearPasswordDto>, ResetearPasswordUsuarioValidator>();
            services.AddScoped<IValidator<CambiarRolDto>, CambiarRolUsuarioValidator>();
            services.AddScoped<IValidator<ActualizarMiPerfilDto>, ActualizarMiPerfilValidator>();

            // Validadores DTO — Auth
            services.AddScoped<IValidator<LoginDto>, LoginValidator>();

            // Validadores DTO — Clientes
            services.AddScoped<IValidator<CreateClienteDto>, CreateClienteValidator>();
            services.AddScoped<IValidator<UpdateClienteDto>, UpdateClienteValidator>();

            return services;
        }
    }
}
