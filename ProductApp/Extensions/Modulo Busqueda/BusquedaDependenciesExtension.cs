using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Interface.IMappers.Modulo_Busqueda;
using ProductApp.Aplication.Mappers.Modulo_Busqueda;
using ProductApp.Aplication.Services;

namespace ProductApp.Extensions.Modulo_Busqueda
{
    public static class BusquedaDependenciesExtension
    {
        public static IServiceCollection AddModuloBusqueda(this IServiceCollection services)
        {
            // Sin repositorios propios: BusquedaService reutiliza IProductoRepository,
            // IClienteRepository, IOrdenRepository e IProveedorRepository, que ya registran
            // sus módulos correspondientes.

            // Mappers
            services.AddScoped<IMapperBusqueda, BusquedaMapper>();

            // Servicios
            services.AddScoped<IBusquedaServices, BusquedaService>();

            return services;
        }
    }
}
