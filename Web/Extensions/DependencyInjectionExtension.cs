using Web.Extensions.Modulo_Productos;
using Web.Extensions.Modulo_Reportes;
using Web.Extensions.Modulo_Usuarios;
using Web.Extensions.Modulo_Ventas;

namespace Web.Extensions
{
    public static class DependencyInjectionExtension
    {
        public static IServiceCollection AddWebDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddInfraestructura(configuration);
            services.AddModuloUsuarios();
            services.AddModuloProductos();
            services.AddModuloVentas();
            services.AddModuloReportes();

            return services;
        }
    }
}
