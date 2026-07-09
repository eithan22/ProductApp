using Web.Services.EndPoints.Modulo_Reportes;
using Web.Services.Interfaces.IEndPoints.Modulo_Reportes;
using Web.Services.Interfaces.ServicesHttp.Modulo_Reportes;
using Web.Services.ServicesHttp.Modulo_Reportes;

namespace Web.Extensions.Modulo_Reportes
{
    public static class ReportesDependenciesExtension
    {
        public static IServiceCollection AddModuloReportes(this IServiceCollection services)
        {
            services.AddScoped<IReporteHttpServices, ReporteHttpServices>();
            services.AddScoped<IReporteEndpoint, ReporteEndpoint>();

            return services;
        }
    }
}
