using Web.Services.EndPoints.Modulo_Busqueda;
using Web.Services.Interfaces.IEndPoints.Modulo_Busqueda;
using Web.Services.Interfaces.ServicesHttp.Modulo_Busqueda;
using Web.Services.ServicesHttp.Modulo_Busqueda;

namespace Web.Extensions.Modulo_Busqueda
{
    public static class BusquedaDependenciesExtension
    {
        public static IServiceCollection AddModuloBusqueda(this IServiceCollection services)
        {
            services.AddScoped<IBusquedaHttpServices, BusquedaHttpServices>();
            services.AddScoped<IBusquedaEndpoint, BusquedaEndpoint>();

            return services;
        }
    }
}
