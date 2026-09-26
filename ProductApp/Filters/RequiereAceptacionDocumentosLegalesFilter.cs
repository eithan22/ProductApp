using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProductApp.Aplication.Interface.Servicios.Modulo_Usuarios;
using ProductApp.Aplication.Result.ApiResponses;
using System.Security.Claims;

namespace ProductApp.Api.Filters
{
    // Espejo de RequiereCambioPasswordFilter, con una diferencia deliberada: no lee un claim del
    // token sino la base, delegando en IVerificadorAceptacionDocumentosLegalesService (patrón de
    // VerificarSesionVigenteFilter). El motivo es que este estado cambia DENTRO de la sesión: el
    // usuario acepta y tiene que poder seguir trabajando con el mismo token. Con un claim habría
    // que cerrarle la sesión y hacerlo entrar de nuevo, como hace el cambio de contraseña, donde
    // el re-login es natural porque la credencial cambió; acá no lo sería.
    //
    // Costo: una consulta proyectada de una columna por petición autenticada, el mismo que ya se
    // paga por VerificarSesionVigenteFilter.
    public class RequiereAceptacionDocumentosLegalesFilter : IAsyncActionFilter
    {
        private readonly IVerificadorAceptacionDocumentosLegalesService _verificadorAceptacion;

        public RequiereAceptacionDocumentosLegalesFilter(
            IVerificadorAceptacionDocumentosLegalesService verificadorAceptacion)
        {
            _verificadorAceptacion = verificadorAceptacion;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // La comprobación de IsAuthenticated no es decorativa (RequiereCambioPasswordFilter
            // no la necesita porque un anónimo simplemente no trae el claim): acá, sin ella, cada
            // petición anónima haría una consulta con id 0 y volvería con 403.
            if (context.HttpContext.User.Identity?.IsAuthenticated == true)
            {
                var permiteSinAceptar = context.ActionDescriptor.EndpointMetadata
                    .OfType<PermitirSinAceptarDocumentosLegalesAttribute>()
                    .Any();

                if (!permiteSinAceptar)
                {
                    // Si el claim falta o no es un entero, usuarioId queda en 0 y el servicio lo
                    // rechaza igual: el mensaje se decide en un solo lugar, la capa de aplicación.
                    int.TryParse(context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var usuarioId);

                    var resultado = await _verificadorAceptacion.VerificarAceptacionAsync(usuarioId);

                    if (!resultado.IsSuccess)
                    {
                        context.Result = new ObjectResult(ApiResponse.FailureResponse(resultado.Message))
                        {
                            StatusCode = StatusCodes.Status403Forbidden
                        };
                        return;
                    }
                }
            }

            await next();
        }
    }
}
