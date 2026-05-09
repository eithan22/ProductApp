using Web.Services.Base;
using Web.Services.EndPoints;
using Web.Services.EndPoints.Modulo_Productos;
using Web.Services.EndPoints.Modulo_Usuarios;
using Web.Services.Interfaces.IBase;
using Web.Services.Interfaces.IEndPoints;
using Web.Services.Interfaces.IEndPoints.Modulo_Productos;
using Web.Services.Interfaces.IEndPoints.Modulo_Usuarios;
using Web.Services.Interfaces.ServicesHttp;
using Web.Services.Interfaces.ServicesHttp.Modulo_Productos;
using Web.Services.Interfaces.ServicesHttp.Modulo_Usuarios;
using Web.Services.ServicesHttp;
using Web.Services.ServicesHttp.Modulo_Productos;
using Web.Services.ServicesHttp.Modulo_Usuarios;


namespace Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddHttpClient("BaseUrl", client =>
            {
                var baseUrl = builder.Configuration["ApiSettings:BaseUrl"];
                client.BaseAddress = new Uri(baseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });


            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(60); // duración
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });


            // Inyección de dependencias para servicios HTTP y endpoints
            builder.Services.AddScoped<IBaseHttpServices, BaseHttpServices>();
            builder.Services.AddScoped<IClienteHttpServices, ClienteHttpServices>();
            builder.Services.AddScoped<IAuthHttpServices, AuthHttpServices>();
            builder.Services.AddScoped<IUsuarioHttpServices, UsuarioHttpServices>();
            builder.Services.AddScoped<ICategoriaHttpServices, CategoriaHttpServices>();


            builder.Services.AddScoped<IClienteEndpoint , ClienteEndpoint>();
            builder.Services.AddScoped<IAuthEndpoint, AuthEndpoint>();
            builder.Services.AddScoped<IUsuariosEndpoint, UsuarioEndpoint>();
            builder.Services.AddScoped<ICategoriaEndpoint, CategoriaEmdpoint>();




            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
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
