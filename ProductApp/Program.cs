using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using ProductApp.Api.Filters;
using ProductApp.Api.Seed;
using ProductApp.Aplication.Result.ApiResponses;
using ProductApp.Extensions;
using ProductApp.Infraesctructura.Persistencia.Contex;
using Serilog;
using System.Threading.RateLimiting;

namespace ProductApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // QuestPDF exige declarar la licencia una sola vez antes de generar cualquier
            // documento. El proyecto califica para la licencia Community (gratuita).
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            var builder = WebApplication.CreateBuilder(args);

            // No revelar el stack tecnológico en cada respuesta HTTP.
            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.AddServerHeader = false;
            });

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
                // El orden importa y va de lo más fundamental a lo más específico:
                //   1. ¿La cuenta sigue existiendo y siendo la que dice el token? (401)
                //   2. ¿Aceptó el contrato que rige el uso del sistema? (403)
                //   3. ¿Su credencial es propia y no la temporal que le dio el administrador? (403)
                // No tiene sentido pedirle nada a una cuenta ya desactivada, y no tiene sentido
                // dejar que una cuenta opere —ni que cambie su propia contraseña— antes de aceptar
                // los términos: es justo lo que la aceptación tiene que preceder.
                options.Filters.Add<VerificarSesionVigenteFilter>();
                options.Filters.Add<RequiereAceptacionDocumentosLegalesFilter>();
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
                options.AddPolicy("login", httpContext =>
                {
                    var clave = httpContext.Connection.RemoteIpAddress?.ToString() ?? "desconocida";
                    return RateLimitPartition.GetFixedWindowLimiter(clave, _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    });
                });

                // Cuando se excede el límite, responde con el mismo formato ApiResponseT que usa el resto de la API.
                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
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

            // Debe ir antes que cualquier middleware que use la IP del cliente. Restringido a
            // loopback a propósito (todavía no hay balanceador real en producción): la app solo
            // confía en X-Forwarded-For si la petición llega directo desde localhost, así nadie
            // externo puede falsificar su IP para evadir el rate-limit de login de arriba.
            // TODO: cuando se agregue el balanceador de Azure, sumar su IP/red real acá con
            // fhOptions.KnownProxies.Add(IPAddress.Parse("<ip-del-balanceador>")).
            var fhOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
                ForwardLimit = 1
            };
            app.UseForwardedHeaders(fhOptions);

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

            // Headers de seguridad básicos (OWASP): mitigan MIME-sniffing y clickjacking.
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Append("X-Frame-Options", "DENY");
                context.Response.Headers.Append("Referrer-Policy", "no-referrer");
                await next();
            });

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
