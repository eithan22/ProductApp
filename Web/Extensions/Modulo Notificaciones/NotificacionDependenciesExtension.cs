using Web.Services.EndPoints.Modulo_Notificaciones;
using Web.Services.Interfaces.IEndPoints.Modulo_Notificaciones;
using Web.Services.Interfaces.ServicesHttp.Modulo_Notificaciones;
using Web.Services.ServicesHttp.Modulo_Notificaciones;

namespace Web.Extensions.Modulo_Notificaciones
{
    public static class NotificacionDependenciesExtension
    {
        public static IServiceCollection AddModuloNotificaciones(this IServiceCollection services)
        {
            services.AddScoped<INotificacionHttpServices, NotificacionHttpServices>();
            services.AddScoped<INotificacionEndpoint, NotificacionEndpoint>();

            return services;
        }
    }
}
