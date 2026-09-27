using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProductApp.Aplication.Interface.Servicios.Modulo_Usuarios;
using ProductApp.Aplication.Result.ApiResponses;
using System.Security.Claims;

namespace ProductApp.Api.Filters
{
    // Complemento obligatorio del JWT: el token es inmutable hasta que vence, así que
    // desactivar un usuario o cambiarle el rol no se refleja solo. Este filtro revalida
    // contra la base, en cada petición autenticada, que el usuario del token siga activo,
    // no esté eliminado y conserve el mismo rol. El precio es una consulta proyectada de
    // dos columnas por petición; la alternativa (denylist de tokens o refresh tokens)
    // exigía almacenamiento propio y limpieza para el mismo resultado.
    public class VerificarSesionVigenteFilter : IAsyncActionFilter
    {
        private readonly IVerificadorSesionService _verificadorSesionService;

        public VerificarSesionVigenteFilter(IVerificadorSesionService verificadorSesionService)
        {
            _verificadorSesionService = verificadorSesionService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                var permiteSinVerificar = context.ActionDescriptor.EndpointMetadata
                    .OfType<PermitirSinVerificarEstadoAttribute>()
                    .Any();

                if (!permiteSinVerificar)
                {
                    // Si el claim falta o no es un entero, usuarioId queda en 0 y el servicio
                    // lo rechaza igual: así el mensaje de sesión inválida se decide en un
                    // solo lugar (la capa de aplicación) y no se duplica acá.
                    int.TryParse(context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var usuarioId);
                    var rolEnElToken = context.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;

                    var resultado = await _verificadorSesionService.VerificarSesionAsync(usuarioId, rolEnElToken);

                    if (!resultado.IsSuccess)
                    {
                        context.Result = new ObjectResult(ApiResponse.FailureResponse(resultado.Message))
                        {
                            StatusCode = StatusCodes.Status401Unauthorized
                        };
                        return;
                    }
                }
            }

            await next();
        }
    }
}
