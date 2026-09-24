using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Web.Extensions;
using Web.Filters;

namespace Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // No revelar el stack tecnológico en cada respuesta HTTP.
            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.AddServerHeader = false;
            });

            builder.Services.AddWebDependencies(builder.Configuration);

            // Add services to the container.
            builder.Services.AddControllersWithViews(options =>
            {
                options.Filters.Add<HandleApiErrorsFilter>();
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });

            var app = builder.Build();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            // Content-Security-Policy. script-src queda en 'self': ninguna librería se carga
            // desde un CDN, todas viven en wwwroot/lib, así que un script remoto inyectado
            // no se ejecuta. 'unsafe-inline' es deuda consciente: las vistas de Reporte y el
            // dashboard tienen <script> en línea para armar los gráficos; migrar a nonce exige
            // tocar esas vistas y el onchange en línea de Producto/Index.
            // img-src acepta https: porque las imágenes de producto se sirven desde el blob de
            // Azure, cuyo host depende del despliegue y la Web no lo conoce. En desarrollo se
            // suma Azurite, que responde por http en 127.0.0.1.
            var origenesImagenes = app.Environment.IsDevelopment()
                ? "'self' data: https: http://127.0.0.1:10000"
                : "'self' data: https:";

            var csp = string.Join("; ", new[]
            {
                "default-src 'self'",
                "script-src 'self' 'unsafe-inline'",
                "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com",
                "font-src 'self' https://fonts.gstatic.com",
                $"img-src {origenesImagenes}",
                "connect-src 'self'",
                "form-action 'self'",
                "frame-ancestors 'none'",
                "base-uri 'self'",
                "object-src 'none'"
            });

            // Headers de seguridad básicos (OWASP): mitigan MIME-sniffing y clickjacking.
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Append("X-Frame-Options", "DENY");
                context.Response.Headers.Append("Referrer-Policy", "no-referrer");
                context.Response.Headers.Append("Content-Security-Policy", csp);
                await next();
            });

            app.UseSession();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
             name: "default",
              pattern: "{controller=Auth}/{action=Login}/{id?}");



            app.Run();
        }
    }
}
