using Microsoft.AspNetCore.Mvc;

namespace ProductApp.Api.Controllers.Modulo_Productos
{
    public class ProductoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
