using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Interface.IMappers.Modulo_Reportes;
using ProductApp.Aplication.Mappers.Modulo_Reportes;
using ProductApp.Aplication.Services;
using ProductApp.Domian.Interfaces;
using ProductApp.Infraesctructura.Persistencia.Repository;

namespace ProductApp.Extensions.Modulo_Reportes
{
    public static class ReportesDependenciesExtension
    {
        public static IServiceCollection AddModuloReportes(this IServiceCollection services)
        {
            services.AddScoped<IReporteRepository, ReporteRepository>();
            services.AddScoped<IMapperReporte, ReporteMapper>();
            services.AddScoped<IReporteServices, ReporteService>();

            return services;
        }
    }
}
