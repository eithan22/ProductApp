using Web.Extensions.Modulo_Busqueda;
using Web.Extensions.Modulo_Configuracion;
using Web.Extensions.Modulo_Notificaciones;
using Web.Extensions.Modulo_Productos;
using Web.Extensions.Modulo_Proveedores;
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
            services.AddModuloNotificaciones();
            services.AddModuloUsuarios();
            services.AddModuloProductos();
            services.AddModuloProveedores();
            services.AddModuloVentas();
            services.AddModuloReportes();
            services.AddModuloConfiguracion();
            services.AddModuloBusqueda();

            return services;
        }
    }
}
