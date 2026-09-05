using Microsoft.AspNetCore.Mvc;
using ProductApp.Aplication.Common;
using Web.Models.Modelo_Productos.ProductoModels;
using Web.Services.Interfaces.ServicesHttp.Modulo_Productos;

namespace Web.Controllers.Modulo_Productos
{
    public class ProductoController : Controller
    {
        private readonly IProductoHttpServices _productoHttpServices;
        private readonly ICategoriaHttpServices _categoriaHttpServices;
        private readonly ILogger<ProductoController> _logger;

        public ProductoController(IProductoHttpServices productoHttpServices,
            ICategoriaHttpServices categoriaHttpServices,
            ILogger<ProductoController> logger)
        {
            _productoHttpServices = productoHttpServices;
            _categoriaHttpServices = categoriaHttpServices;
            _logger = logger;
        }

        public async Task<ActionResult> Index(bool incluirInactivos = false, int pageNumber = 1)
        {
            var result = await _productoHttpServices.GetProductosAsync(incluirInactivos, pageNumber);
            ViewBag.IncluirInactivos = incluirInactivos;
            return View(result);
        }

        [HttpGet]
        public async Task<ActionResult> Buscar(string? nombre, string? categoria)
        {
            try
            {
                var result = await _productoHttpServices.BuscarProductosAsync(nombre, categoria);
                var paged = new PagedResult<ProductoModel> { Items = result, PageNumber = 1, PageSize = result.Count, TotalCount = result.Count };
                return View("Index", paged);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", new PagedResult<ProductoModel>());
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
        public async Task<ActionResult> Create(CreateProductoModel model, IFormFile? imagen)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var creado = await _productoHttpServices.CreateProductoAsync(model);
                    await SubirImagenAsync(creado.Id, imagen);
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
                ViewBag.ImagenUrl = producto.ImagenUrl;
                return View(model);
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(UpdateProductoModel model, IFormFile? imagen, string? imagenActual)
        {
            ViewBag.ImagenUrl = imagenActual;

            try
            {
                if (ModelState.IsValid)
                {
                    await _productoHttpServices.UpdateProductoAsync(model);
                    await SubirImagenAsync(model.Id, imagen);
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

        // La imagen se sube en un segundo paso porque el endpoint necesita el Id
        // del producto y la API sólo acepta multipart, no el JSON del formulario.
        private async Task SubirImagenAsync(int productoId, IFormFile? imagen)
        {
            if (imagen is null || imagen.Length == 0)
                return;

            try
            {
                await using var contenido = imagen.OpenReadStream();
                await _productoHttpServices.SubirImagenAsync(productoId, contenido, imagen.FileName, imagen.ContentType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo subir la imagen del producto {ProductoId}", productoId);
                TempData["Aviso"] = $"El producto se guardó, pero la imagen no se pudo subir: {ex.Message}";
            }
        }

        private async Task CargarCategorias()
        {
            ViewBag.Categorias = await _categoriaHttpServices.GetCategoriasAsync();
        }
    }
}
