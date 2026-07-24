using Microsoft.AspNetCore.Diagnostics;
using ProductApp.Aplication.Result.ApiResponses;
using ProductApp.Domian.Common.Exceptions;

namespace ProductApp.Api.Filters
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is DomainException domainEx)
            {
                _logger.LogWarning(domainEx, "Excepción de dominio en {Path}", httpContext.Request.Path);
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsJsonAsync(
                    ApiResponseT<object>.FailureResponse(domainEx.Message), cancellationToken);
                return true;
            }

            _logger.LogError(exception, "Excepción no controlada en {Path}", httpContext.Request.Path);
            httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(
                ApiResponseT<object>.FailureResponse("Ocurrió un error inesperado. Intenta nuevamente más tarde."),
                cancellationToken);
            return true;
        }
    }
}
