using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProductApp.Aplication.Result.ApiResponses;

namespace ProductApp.Api.Filters
{
    public class RequiereCambioPasswordFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var permiteConPendiente = context.ActionDescriptor.EndpointMetadata
                .OfType<PermitirConPasswordPendienteAttribute>()
                .Any();

            if (!permiteConPendiente)
            {
                var claim = context.HttpContext.User.FindFirst("DebeCambiarPassword");
                if (claim != null && bool.TryParse(claim.Value, out var debeCambiar) && debeCambiar)
                {
                    context.Result = new ObjectResult(ApiResponse.FailureResponse("Debe cambiar su contraseña antes de continuar"))
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                    return;
                }
            }

            await next();
        }
    }
}
