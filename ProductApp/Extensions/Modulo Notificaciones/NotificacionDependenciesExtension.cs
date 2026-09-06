using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Interface.IMappers.Modulo_Notificaciones;
using ProductApp.Aplication.Mappers.Modulo_Notificaciones;
using ProductApp.Aplication.Services;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Repository;

namespace ProductApp.Extensions.Modulo_Notificaciones
{
    public static class NotificacionDependenciesExtension
    {
        public static IServiceCollection AddModuloNotificaciones(this IServiceCollection services)
        {
            services.AddScoped<INotificacionRepository, NotificacionRepository>();
            services.AddScoped<IMapperNotificacion, NotificacionMapper>();
            services.AddScoped<INotificacionServices, NotificacionService>();

            return services;
        }
    }
}
