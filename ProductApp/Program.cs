using ProductApp.Api.Filters;
using ProductApp.Api.Seed;
using ProductApp.Extensions;
using ProductApp.Infraesctructura.Persistencia.Contex;

namespace ProductApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException(
                    "Falta configurar ConnectionStrings:DefaultConnection. En desarrollo, usa " +
                    "dotnet user-secrets set \"ConnectionStrings:DefaultConnection\" \"...\". " +
                    "En producción, configura la variable de entorno ConnectionStrings__DefaultConnection.");

            if (string.IsNullOrWhiteSpace(builder.Configuration["Jwt:Key"]))
                throw new InvalidOperationException(
                    "Falta configurar Jwt:Key. Ver dotnet user-secrets (desarrollo) o la " +
                    "variable de entorno Jwt__Key (producción).");

            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<RequiereCambioPasswordFilter>();
            });
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Pon aquí el token así: Bearer {tu_token}"
                });

                options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            builder.Services.AddProjectDependencies(builder.Configuration);

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                await DbInitializer.SeedAsync(context, configuration);
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseExceptionHandler();
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
