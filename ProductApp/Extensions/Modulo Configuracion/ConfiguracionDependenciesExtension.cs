using FluentValidation;
using ProductApp.Aplication.Dtos.Modulo_Configuracion;
using ProductApp.Aplication.Interface.IMappers.Modulo_Configuracion;
using ProductApp.Aplication.Interface.Servicios.Modulo_Configuracion;
using ProductApp.Aplication.Mappers.Modulo_Configuracion;
using ProductApp.Aplication.Services.Modulo_Configuracion;
using ProductApp.Aplication.Validators.Modulo_Configuracion;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Repository;

namespace ProductApp.Extensions.Modulo_Configuracion
{
    public static class ConfiguracionDependenciesExtension
    {
        public static IServiceCollection AddModuloConfiguracion(this IServiceCollection services)
        {
            services.AddScoped<IConfiguracionSistemaRepository, ConfiguracionSistemaRepository>();
            services.AddScoped<IMapperConfiguracionSistema, ConfiguracionSistemaMapper>();
            services.AddScoped<IConfiguracionSistemaService, ConfiguracionSistemaService>();
            services.AddScoped<IValidator<ActualizarConfiguracionSistemaDto>, ActualizarConfiguracionSistemaValidator>();

            return services;
        }
    }
}
