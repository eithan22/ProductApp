using Microsoft.AspNetCore.Mvc;
using ProductApp.Aplication.Common;
using System.Net;
using Web.Models.Modelo_Productos.ProductoModels;
using Web.Services.Base;
using Web.Services.Interfaces.ServicesHttp.Modulo_Productos;
using Web.Services.Interfaces.ServicesHttp.Modulo_Proveedores;

namespace Web.Controllers.Modulo_Productos
{
    public class ProductoController : Controller
    {
        private readonly IProductoHttpServices _productoHttpServices;
        private readonly ICategoriaHttpServices _categoriaHttpServices;
        private readonly IProveedorHttpServices _proveedorHttpServices;
        private readonly ILogger<ProductoController> _logger;

        public ProductoController(IProductoHttpServices productoHttpServices,
            ICategoriaHttpServices categoriaHttpServices,
            IProveedorHttpServices proveedorHttpServices,
            ILogger<ProductoController> logger)
        {
            _productoHttpServices = productoHttpServices;
            _categoriaHttpServices = categoriaHttpServices;
            _proveedorHttpServices = proveedorHttpServices;
            _logger = logger;
        }

        public async Task<ActionResult> Index(bool incluirInactivos = false, int pageNumber = 1)
        {
            var result = await _productoHttpServices.GetProductosAsync(incluirInactivos, pageNumber);
            ViewBag.IncluirInactivos = incluirInactivos;
            ViewBag.EsAdministrador = EsAdministrador();
            await CargarCategorias();
            return View(result);
        }

        [HttpGet]
        public async Task<ActionResult> Buscar(string? nombre, string? categoria)
        {
            ViewBag.NombreBuscado = nombre;
            ViewBag.CategoriaSeleccionada = categoria;
            ViewBag.EsAdministrador = EsAdministrador();
            await CargarCategorias();

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
            ViewBag.EsAdministrador = EsAdministrador();
            return View(result);
        }

        public async Task<ActionResult> Create()
        {
            await CargarCategorias();
            await CargarProveedores();
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
                await CargarProveedores();
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await CargarCategorias();
                await CargarProveedores();
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
                    CategoriaId = categoriaActual?.Id ?? 0,
                    // La API devuelve el id del proveedor directamente, así que acá no
                    // hace falta el rodeo de buscarlo por nombre como con la categoría.
                    ProveedorId = producto.ProveedorId
                };

                ViewBag.Categorias = categorias;
                await CargarProveedores();
                // Si el proveedor asignado ya está dado de baja no viene en la lista de
                // activos: el nombre viaja aparte para poder conservarlo en el select.
                ViewBag.ProveedorActual = producto.Proveedor;
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
                await CargarProveedores();
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await CargarCategorias();
                await CargarProveedores();
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Disable(int id)
        {
            if (!EsAdministrador())
                return SinPermiso();

            await _productoHttpServices.DisableProductoAsync(id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Enable(int id)
        {
            if (!EsAdministrador())
                return SinPermiso();

            await _productoHttpServices.EnableProductoAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // ---------- Importación masiva (RF-3.10) ----------
        // Solo Administrador, igual que la API. Acá el chequeo es contra el rol en sesión
        // (mismo patrón que ConfiguracionController): la Web no autentica con [Authorize],
        // el JWT vive en Session y el atributo de ASP.NET no lo ve.

        [HttpGet]
        public ActionResult Importar()
        {
            if (!EsAdministrador())
                return SinPermiso();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<ActionResult> Importar(IFormFile? archivo)
        {
            if (!EsAdministrador())
                return SinPermiso();

            if (archivo is null || archivo.Length == 0)
            {
                ModelState.AddModelError("", "Adjuntá un archivo .xlsx o .csv para importar.");
                return View();
            }

            try
            {
                await using var contenido = archivo.OpenReadStream();
                var resultado = await _productoHttpServices.ImportarMasivoAsync(
                    contenido, archivo.FileName, archivo.ContentType);

                return View("ImportarResultado", resultado);
            }
            catch (ApiHttpException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Se deja propagar para que HandleApiErrorsFilter cierre la sesión.
                throw;
            }
            catch (Exception ex)
            {
                // Archivo sin las columnas esperadas, más de 500 filas, formato no admitido:
                // todo eso llega acá como el mensaje real que escribió la API. Se muestra en
                // la misma pantalla para no perder el contexto de los tres pasos.
                _logger.LogError(ex, "Falló la importación masiva del archivo {Archivo}", archivo.FileName);
                ModelState.AddModelError("", ex.Message);
                return View();
            }
        }

        [HttpGet]
        public async Task<ActionResult> DescargarPlantillaImportacion()
        {
            if (!EsAdministrador())
                return SinPermiso();

            try
            {
                var (contenido, nombre) = await _productoHttpServices.DescargarPlantillaImportacionAsync();
                return File(contenido, ContenidoXlsx, nombre ?? "plantilla-importacion-productos.xlsx");
            }
            catch (ApiHttpException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Vuelve a Importar, no al catálogo: el usuario estaba en medio de los pasos.
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Importar));
            }
        }

        // El .xlsx de filas rechazadas ya vino en la respuesta de ImportarMasivo, en base64:
        // no se le vuelve a pedir nada a la API. La vista lo reenvía y acá solo se decodifica,
        // para que la descarga salga con File(...) como todas las demás del sistema.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DescargarErroresImportacion(string? archivoBase64, string? nombreArchivo)
        {
            if (!EsAdministrador())
                return SinPermiso();

            if (string.IsNullOrWhiteSpace(archivoBase64))
            {
                TempData["Error"] = "No hay un archivo de errores para descargar.";
                return RedirectToAction(nameof(Importar));
            }

            byte[] contenido;

            try
            {
                contenido = Convert.FromBase64String(archivoBase64);
            }
            catch (FormatException)
            {
                TempData["Error"] = "El archivo de errores no se pudo reconstruir. Volvé a importar el archivo.";
                return RedirectToAction(nameof(Importar));
            }

            return File(contenido, ContenidoXlsx, string.IsNullOrWhiteSpace(nombreArchivo)
                ? "errores-importacion.xlsx"
                : nombreArchivo);
        }

        private const string ContenidoXlsx =
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        private bool EsAdministrador() =>
            HttpContext.Session.GetString("ROL") == "Administrador";

        private ActionResult SinPermiso(string mensaje = "No tenés permisos para realizar esta acción.")
        {
            TempData["Error"] = mensaje;
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

        // Solo los activos: a un proveedor dado de baja no se le asignan productos nuevos.
        private async Task CargarProveedores()
        {
            ViewBag.Proveedores = await _proveedorHttpServices.GetProveedoresActivosAsync();
        }
    }
}
