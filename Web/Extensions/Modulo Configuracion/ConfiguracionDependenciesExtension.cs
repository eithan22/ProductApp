using Web.Services.EndPoints.Modulo_Configuracion;
using Web.Services.Interfaces.IEndPoints.Modulo_Configuracion;
using Web.Services.Interfaces.ServicesHttp.Modulo_Configuracion;
using Web.Services.ServicesHttp.Modulo_Configuracion;

namespace Web.Extensions.Modulo_Configuracion
{
    public static class ConfiguracionDependenciesExtension
    {
        public static IServiceCollection AddModuloConfiguracion(this IServiceCollection services)
        {
            services.AddScoped<IConfiguracionHttpServices, ConfiguracionHttpServices>();
            services.AddScoped<IConfiguracionEndpoint, ConfiguracionEndpoint>();

            return services;
        }
    }
}
