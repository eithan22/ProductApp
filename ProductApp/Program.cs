using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using ProductApp.Api.Filters;
using ProductApp.Api.Seed;
using ProductApp.Aplication.Result.ApiResponses;
using ProductApp.Extensions;
using ProductApp.Infraesctructura.Persistencia.Contex;
using Serilog;

namespace ProductApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Reemplaza el logging por consola por defecto: mismos niveles que Serilog:MinimumLevel
            // en appsettings, pero ahora también persistidos en logs/ con rotación diaria.
            builder.Host.UseSerilog((context, configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .WriteTo.Console()
                    .WriteTo.File(
                        path: "logs/productapp-.log",
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 30,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}");
            });

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

            if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("AzureStorage")))
                throw new InvalidOperationException(
                    "Falta configurar ConnectionStrings:AzureStorage (almacenamiento de imágenes de producto). " +
                    "En desarrollo, usa dotnet user-secrets set \"ConnectionStrings:AzureStorage\" \"UseDevelopmentStorage=true\" " +
                    "con Azurite corriendo. En producción, configura la variable de entorno " +
                    "ConnectionStrings__AzureStorage con la cadena de conexión de la cuenta de Azure Storage.");

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

            // Limita los intentos de login para frenar fuerza bruta: máximo 5 intentos por minuto,
            // sin cola de espera (el intento número 6 se rechaza al instante con 429, no espera turno).
            builder.Services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("login", limiterOptions =>
                {
                    limiterOptions.PermitLimit = 5;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.QueueLimit = 0;
                });

                // Cuando se excede el límite, responde con el mismo formato ApiResponseT que usa el resto de la API.
                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.ContentType = "application/json";
                    await context.HttpContext.Response.WriteAsJsonAsync(
                        ApiResponseT<object>.FailureResponse(
                            "Demasiados intentos de inicio de sesión. Intenta nuevamente en un minuto."),
                        cancellationToken);
                };
            });

            // Health check con chequeo real contra la base de datos (no solo "la app responde").
            builder.Services.AddHealthChecks()
                .AddDbContextCheck<AppDbContext>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Aplica migraciones pendientes al arrancar; si ya están al día, no hace nada.
                await context.Database.MigrateAsync();

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
            app.UseRateLimiter(); // aplica la política de rate limiting a las rutas que la declaren con [EnableRateLimiting]
            app.MapControllers();

            // Sin [Authorize]: el balanceador de carga externo debe poder consultarlo sin token.
            app.MapHealthChecks("/health");

            app.Run();
        }
    }
}
