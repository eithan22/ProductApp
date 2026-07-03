using Web.Services.EndPoints;
using Web.Services.EndPoints.Modulo_Usuarios;
using Web.Services.Interfaces.IEndPoints;
using Web.Services.Interfaces.IEndPoints.Modulo_Usuarios;
using Web.Services.Interfaces.ServicesHttp;
using Web.Services.Interfaces.ServicesHttp.Modulo_Usuarios;
using Web.Services.ServicesHttp;
using Web.Services.ServicesHttp.Modulo_Usuarios;

namespace Web.Extensions.Modulo_Usuarios
{
    public static class UsuarioDependenciesExtension
    {
        public static IServiceCollection AddModuloUsuarios(this IServiceCollection services)
        {
            services.AddScoped<IClienteHttpServices, ClienteHttpServices>();
            services.AddScoped<IAuthHttpServices, AuthHttpServices>();
            services.AddScoped<IUsuarioHttpServices, UsuarioHttpServices>();

            services.AddScoped<IClienteEndpoint, ClienteEndpoint>();
            services.AddScoped<IAuthEndpoint, AuthEndpoint>();
            services.AddScoped<IUsuariosEndpoint, UsuarioEndpoint>();

            return services;
        }
    }
}
