using Web.Services.EndPoints.Modulo_Ventas;
using Web.Services.Interfaces.IEndPoints.Modulo_Ventas;
using Web.Services.Interfaces.ServicesHttp.Modulo_Ventas;
using Web.Services.ServicesHttp.Modulo_Ventas;

namespace Web.Extensions.Modulo_Ventas
{
    public static class VentasDependenciesExtension
    {
        public static IServiceCollection AddModuloVentas(this IServiceCollection services)
        {
            services.AddScoped<IOrdenHttpServices, OrdenHttpServices>();
            services.AddScoped<IOrdenEndpoint, OrdenEndpoint>();

            services.AddScoped<IDetalleOrdenHttpServices, DetalleOrdenHttpServices>();
            services.AddScoped<IDetalleOrdenEndpoint, DetalleOrdenEndpoint>();

            services.AddScoped<IPagoHttpServices, PagoHttpServices>();
            services.AddScoped<IPagoEndpoint, PagoEndpoint>();

            return services;
        }
    }
}
