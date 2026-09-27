using ProductApp.Extensions.Modulo_Busqueda;
using ProductApp.Extensions.Modulo_Configuracion;
using ProductApp.Extensions.Modulo_Notificaciones;
using ProductApp.Extensions.Modulo_Productos;
using ProductApp.Extensions.Modulo_Proveedores;
using ProductApp.Extensions.Modulo_Reportes;
using ProductApp.Extensions.Modulo_Usuarios;
using ProductApp.Extensions.Modulo_Ventas;

namespace ProductApp.Extensions
{
    public static class DependencyInjectionExtension
    {
        public static IServiceCollection AddProjectDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddInfraestructura(configuration);
            services.AddModuloNotificaciones();
            services.AddModuloUsuarios();
            services.AddModuloProductos(configuration);
            services.AddModuloProveedores();
            services.AddModuloVentas(configuration);
            services.AddModuloReportes();
            services.AddModuloConfiguracion();

            // Va al final: solo consume repositorios que ya registraron los módulos anteriores.
            services.AddModuloBusqueda();

            return services;
        }
    }
}
