using ProductApp.Extensions.Modulo_Productos;
using ProductApp.Extensions.Modulo_Usuarios;
using ProductApp.Extensions.Modulo_Ventas;

namespace ProductApp.Extensions
{
    public static class DependencyInjectionExtension
    {
        public static IServiceCollection AddProjectDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddInfraestructura(configuration);
            services.AddModuloUsuarios();
            services.AddModuloProductos();
            services.AddModuloVentas();

            return services;
        }
    }
}
