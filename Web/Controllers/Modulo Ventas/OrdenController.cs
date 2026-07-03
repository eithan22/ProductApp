using Microsoft.AspNetCore.Mvc;
using Web.Models.Modelo_Ventas.DetalleOrdenModels;
using Web.Models.Modelo_Ventas.OrdenModels;
using Web.Models.Modelo_Ventas.PagoModels;
using Web.Services.Interfaces.ServicesHttp;
using Web.Services.Interfaces.ServicesHttp.Modulo_Productos;
using Web.Services.Interfaces.ServicesHttp.Modulo_Ventas;

namespace Web.Controllers.Modulo_Ventas
{
    public class OrdenController : Controller
    {
        private readonly IOrdenHttpServices _ordenHttpServices;
        private readonly IDetalleOrdenHttpServices _detalleOrdenHttpServices;
        private readonly IClienteHttpServices _clienteHttpServices;
        private readonly IProductoHttpServices _productoHttpServices;
        private readonly IPagoHttpServices _pagoHttpServices;

        public OrdenController(
            IOrdenHttpServices ordenHttpServices,
            IDetalleOrdenHttpServices detalleOrdenHttpServices,
            IClienteHttpServices clienteHttpServices,
            IProductoHttpServices productoHttpServices,
            IPagoHttpServices pagoHttpServices)
        {
            _ordenHttpServices = ordenHttpServices;
            _detalleOrdenHttpServices = detalleOrdenHttpServices;
            _clienteHttpServices = clienteHttpServices;
            _productoHttpServices = productoHttpServices;
            _pagoHttpServices = pagoHttpServices;
        }

        public async Task<ActionResult> Index()
        {
            var result = await _ordenHttpServices.GetOrdenesAsync();
            return View(result);
        }

        [HttpGet]
        public async Task<ActionResult> Buscar(int? clienteId, DateTime? fecha)
        {
            try
            {
                List<OrdenModel> result;
                if (clienteId.HasValue)
                    result = await _ordenHttpServices.GetOrdenesByClienteAsync(clienteId.Value);
                else if (fecha.HasValue)
                    result = await _ordenHttpServices.GetOrdenesByFechaAsync(fecha.Value);
                else
                    result = await _ordenHttpServices.GetOrdenesAsync();

                return View("Index", result);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View("Index", new List<OrdenModel>());
            }
        }

        public async Task<ActionResult> Create()
        {
            ViewBag.Clientes = await _clienteHttpServices.GetClientesAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateOrdenModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var orden = await _ordenHttpServices.CreateOrdenAsync(model);
                    return RedirectToAction(nameof(Details), new { id = orden.Id });
                }
                ViewBag.Clientes = await _clienteHttpServices.GetClientesAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Clientes = await _clienteHttpServices.GetClientesAsync();
                return View(model);
            }
        }

        public async Task<ActionResult> Details(int id)
        {
            var orden = await _ordenHttpServices.GetOrdenByIdAsync(id);
            var detalles = await _detalleOrdenHttpServices.GetDetallesPorOrdenAsync(id);

            ViewBag.Detalles = detalles;
            var productos = await _productoHttpServices.GetProductosAsync();
            ViewBag.Productos = productos.Where(p => p.Estado == "Activo").ToList();

            List<PagoModel> pagos;
            try
            {
                pagos = await _pagoHttpServices.GetPagosPorOrdenAsync(id);
            }
            catch
            {
                pagos = new List<PagoModel>();
            }

            ViewBag.Pagos = pagos;
            ViewBag.SaldoPendiente = orden.Total - pagos.Sum(p => p.Monto);

            return View(orden);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AgregarProducto(int ordenId, int productId, int cantidad)
        {
            try
            {
                await _detalleOrdenHttpServices.AgregarProductoAsync(new CreateDetalleOrdenModel
                {
                    OrdenId = ordenId,
                    ProductId = productId,
                    Cantidad = cantidad
                });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Details), new { id = ordenId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ActualizarCantidad(int detalleId, int ordenId, int cantidad)
        {
            try
            {
                await _detalleOrdenHttpServices.ActualizarCantidadAsync(new UpdateDetalleOrdenModel
                {
                    Id = detalleId,
                    Cantidad = cantidad
                });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Details), new { id = ordenId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EliminarProducto(int detalleId, int ordenId)
        {
            try
            {
                await _detalleOrdenHttpServices.EliminarProductoAsync(detalleId);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Details), new { id = ordenId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CambiarEstado(int id, string nuevoEstado)
        {
            try
            {
                await _ordenHttpServices.CambiarEstadoAsync(new CambiarEstadoOrdenModel { Id = id, NuevoEstado = nuevoEstado });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegistrarPago(int ordenId, decimal monto, string metodoPago)
        {
            try
            {
                await _pagoHttpServices.RegistrarPagoAsync(new CreatePagoModel
                {
                    OrdenId = ordenId,
                    Monto = monto,
                    MetodoPago = metodoPago
                });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Details), new { id = ordenId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Cancelar(int id)
        {
            try
            {
                await _ordenHttpServices.CancelarOrdenAsync(id);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
