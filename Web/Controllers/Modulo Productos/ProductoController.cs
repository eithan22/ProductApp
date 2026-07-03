using Microsoft.AspNetCore.Mvc;
using Web.Models.Modelo_Productos.ProductoModels;
using Web.Services.Interfaces.ServicesHttp.Modulo_Productos;

namespace Web.Controllers.Modulo_Productos
{
    public class ProductoController : Controller
    {
        private readonly IProductoHttpServices _productoHttpServices;
        private readonly ICategoriaHttpServices _categoriaHttpServices;

        public ProductoController(IProductoHttpServices productoHttpServices, ICategoriaHttpServices categoriaHttpServices)
        {
            _productoHttpServices = productoHttpServices;
            _categoriaHttpServices = categoriaHttpServices;
        }

        public async Task<ActionResult> Index()
        {
            var result = await _productoHttpServices.GetProductosAsync();
            return View(result);
        }

        [HttpGet]
        public async Task<ActionResult> Buscar(string? nombre, string? categoria)
        {
            try
            {
                var result = await _productoHttpServices.BuscarProductosAsync(nombre, categoria);
                return View("Index", result);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", new List<ProductoModel>());
            }
        }

        public async Task<ActionResult> Details(int id)
        {
            var result = await _productoHttpServices.GetProductoByIdAsync(id);
            return View(result);
        }

        public async Task<ActionResult> Create()
        {
            await CargarCategorias();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateProductoModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _productoHttpServices.CreateProductoAsync(model);
                    return RedirectToAction(nameof(Index));
                }
                await CargarCategorias();
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await CargarCategorias();
                return View(model);
            }
        }

        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                var producto = await _productoHttpServices.GetProductoByIdAsync(id);
                var categorias = await _categoriaHttpServices.GetCategoriasAsync();
                var categoriaActual = categorias.FirstOrDefault(c => c.Nombre == producto.Categoria);

                var model = new UpdateProductoModel
                {
                    Id = producto.Id,
                    Nombre = producto.Nombre,
                    Descripcion = producto.Descripcion,
                    Precio = producto.Precio,
                    Costo = producto.Costo,
                    CategoriaId = categoriaActual?.Id ?? 0
                };

                ViewBag.Categorias = categorias;
                return View(model);
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(UpdateProductoModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    await _productoHttpServices.UpdateProductoAsync(model);
                    return RedirectToAction(nameof(Index));
                }
                await CargarCategorias();
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await CargarCategorias();
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Disable(int id)
        {
            await _productoHttpServices.DisableProductoAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Enable(int id)
        {
            await _productoHttpServices.EnableProductoAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task CargarCategorias()
        {
            ViewBag.Categorias = await _categoriaHttpServices.GetCategoriasAsync();
        }
    }
}
