using Web.Services.EndPoints.Modulo_Proveedores;
using Web.Services.Interfaces.IEndPoints.Modulo_Proveedores;
using Web.Services.Interfaces.ServicesHttp.Modulo_Proveedores;
using Web.Services.ServicesHttp.Modulo_Proveedores;

namespace Web.Extensions.Modulo_Proveedores
{
    public static class ProveedorDependenciesExtension
    {
        public static IServiceCollection AddModuloProveedores(this IServiceCollection services)
        {
            services.AddScoped<IProveedorHttpServices, ProveedorHttpServices>();
            services.AddScoped<IProveedorEndpoint, ProveedorEndpoint>();

            return services;
        }
    }
}
