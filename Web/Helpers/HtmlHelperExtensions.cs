using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Web.Helpers
{
    public static class HtmlHelperExtensions
    {
        public static string Moneda(this IHtmlHelper html, decimal monto)
        {
            var moneda = html.ViewContext.HttpContext.Session.GetString("MONEDA");
            return MonedaHelper.Formatear(monto, moneda);
        }
    }
}
