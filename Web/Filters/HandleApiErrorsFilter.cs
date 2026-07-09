using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

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

            if (message.Contains("Debe cambiar su contraseña"))
            {
                tempData["Aviso"] = message;
                context.Result = new RedirectToActionResult("CambiarPassword", "Usuario", null);
            }
            else
            {
                tempData["Error"] = message;
                context.Result = new RedirectToActionResult("Index", "Home", null);
            }

            context.ExceptionHandled = true;
        }
    }
}
