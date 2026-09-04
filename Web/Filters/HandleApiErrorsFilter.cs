using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Net;
using Web.Services.Base;

namespace Web.Filters
{
    public class HandleApiErrorsFilter : IExceptionFilter
    {
        private readonly ITempDataDictionaryFactory _tempDataFactory;

        public HandleApiErrorsFilter(ITempDataDictionaryFactory tempDataFactory)
        {
            _tempDataFactory = tempDataFactory;
        }

        public void OnException(ExceptionContext context)
        {
            var tempData = _tempDataFactory.GetTempData(context.HttpContext);
            var message = context.Exception.Message;

            // 401: el token venció o es inválido. Mandarlo a Home sería un bucle,
            // porque Home vuelve a llamar a la API y vuelve a fallar igual.
            if (context.Exception is ApiHttpException { StatusCode: HttpStatusCode.Unauthorized })
            {
                context.HttpContext.Session.Clear();
                tempData["Aviso"] = "Tu sesión expiró. Iniciá sesión de nuevo.";
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                context.ExceptionHandled = true;
                return;
            }

            if (message.Contains("Debe cambiar su contraseña"))
            {
                tempData["Aviso"] = message;
                context.Result = new RedirectToActionResult("CambiarPassword", "Usuario", null);
                context.ExceptionHandled = true;
                return;
            }

            tempData["Error"] = message;

            // Salvavidas estructural: si el que falló YA es Home/Index, redirigir
            // ahí otra vez es un bucle garantizado, sea cual sea la causa
            // (por ejemplo la API caída, que no trae status code).
            var ruta = context.RouteData.Values;
            var esHomeIndex =
                string.Equals(ruta["controller"] as string, "Home", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(ruta["action"] as string, "Index", StringComparison.OrdinalIgnoreCase);

            context.Result = esHomeIndex
                ? new RedirectToActionResult("Error", "Home", null)
                : new RedirectToActionResult("Index", "Home", null);

            context.ExceptionHandled = true;
        }
    }
}
