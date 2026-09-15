using FluentValidation;
using ProductApp.Aplication.BusinessValidator.Modulo_Proveedores;
using ProductApp.Aplication.Dtos.Modulo_Proveedores.ProveedorDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Interface.IMappers.Modulo_Proveedores;
using ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Proveedores;
using ProductApp.Aplication.Mappers.Modulo_Proveedores;
using ProductApp.Aplication.Services;
using ProductApp.Aplication.Validators.Modulo_Proveedores.ProveedorValidator;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Repository;

namespace ProductApp.Extensions.Modulo_Proveedores
{
    public static class ProveedorDependenciesExtension
    {
        public static IServiceCollection AddModuloProveedores(this IServiceCollection services)
        {
            // Repositorios
            services.AddScoped<IProveedorRepository, ProveedorRepository>();

            // Mappers
            services.AddScoped<IMapperProveedor, ProveedorMapper>();

            // Servicios
            services.AddScoped<IProveedorServices, ProveedorService>();

            // Reglas de negocio
            services.AddScoped<IValidatorBusinessProveedor, ValidatorBusinessProveedor>();

            // Validadores DTO
            services.AddScoped<IValidator<CreateProveedorDto>, CreateProveedorValidator>();
            services.AddScoped<IValidator<UpdateProveedorDto>, UpdateProveedorValidator>();

            return services;
        }
    }
}
