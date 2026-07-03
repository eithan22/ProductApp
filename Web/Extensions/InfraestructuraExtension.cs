using Web.Services.Base;
using Web.Services.Interfaces.IBase;

namespace Web.Extensions
{
    public static class InfraestructuraExtension
    {
        public static IServiceCollection AddInfraestructura(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient("BaseUrl", client =>
            {
                var baseUrl = configuration["ApiSettings:BaseUrl"];
                client.BaseAddress = new Uri(baseUrl!);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            services.AddHttpContextAccessor();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.AddScoped<IBaseHttpServices, BaseHttpServices>();

            return services;
        }
    }
}
