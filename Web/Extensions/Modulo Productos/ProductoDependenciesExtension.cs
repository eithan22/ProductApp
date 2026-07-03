using Web.Services.EndPoints.Modulo_Productos;
using Web.Services.Interfaces.IEndPoints.Modulo_Productos;
using Web.Services.Interfaces.ServicesHttp.Modulo_Productos;
using Web.Services.ServicesHttp.Modulo_Productos;

namespace Web.Extensions.Modulo_Productos
{
    public static class ProductoDependenciesExtension
    {
        public static IServiceCollection AddModuloProductos(this IServiceCollection services)
        {
            services.AddScoped<ICategoriaHttpServices, CategoriaHttpServices>();
            services.AddScoped<ICategoriaEndpoint, CategoriaEmdpoint>();

            services.AddScoped<IProductoHttpServices, ProductoHttpServices>();
            services.AddScoped<IProductoEndpoint, ProductoEndpoint>();

            services.AddScoped<IInventarioHttpServices, InventarioHttpServices>();
            services.AddScoped<IInventarioEndpoint, InventarioEndpoint>();

            return services;
        }
    }
}
