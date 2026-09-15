using Microsoft.AspNetCore.Mvc;
using ProductApp.Aplication.Common;
using Web.Models.Modelo_Productos.InventarioModels;
using Web.Services.Interfaces.ServicesHttp.Modulo_Productos;
using Web.Services.Interfaces.ServicesHttp.Modulo_Proveedores;

namespace Web.Controllers.Modulo_Productos
{
    public class InventarioController : Controller
    {
        private readonly IInventarioHttpServices _inventarioHttpServices;
        private readonly IProveedorHttpServices _proveedorHttpServices;

        public InventarioController(IInventarioHttpServices inventarioHttpServices,
            IProveedorHttpServices proveedorHttpServices)
        {
            _inventarioHttpServices = inventarioHttpServices;
            _proveedorHttpServices = proveedorHttpServices;
        }

        public async Task<ActionResult> Index(int pageNumber = 1, int? proveedorId = null)
        {
            var result = await _inventarioHttpServices.GetAllInventariosAsync(pageNumber, 10, proveedorId);
            // El contador de stock bajo respeta el mismo filtro: si se está viendo un
            // proveedor, el número de arriba tiene que hablar de ese proveedor.
            var stockBajo = await _inventarioHttpServices.GetStockBajoAsync(proveedorId);
            ViewBag.SoloBajo = false;
            ViewBag.TotalBajo = stockBajo.Count;
            ViewBag.ProveedorId = proveedorId;
            await CargarProveedores();
            return View(result);
        }

        public async Task<ActionResult> StockBajo(int? proveedorId = null)
        {
            var result = await _inventarioHttpServices.GetStockBajoAsync(proveedorId);
            var paged = new PagedResult<InventarioModel> { Items = result, PageNumber = 1, PageSize = result.Count, TotalCount = result.Count };
            ViewBag.SoloBajo = true;
            ViewBag.TotalBajo = result.Count;
            ViewBag.ProveedorId = proveedorId;
            await CargarProveedores();
            return View("Index", paged);
        }

        public async Task<ActionResult> Movimiento(int productoId)
        {
            var result = await _inventarioHttpServices.GetInventarioPorProductoAsync(productoId);
            return View(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AgregarStock(int productoId, int cantidad)
        {
            try
            {
                await _inventarioHttpServices.AgregarStockAsync(new MovimientoStockModel { ProductoId = productoId, Cantidad = cantidad });
                return RedirectToAction(nameof(Movimiento), new { productoId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var inventario = await _inventarioHttpServices.GetInventarioPorProductoAsync(productoId);
                return View("Movimiento", inventario);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DescontarStock(int productoId, int cantidad)
        {
            try
            {
                await _inventarioHttpServices.DescontarStockAsync(new MovimientoStockModel { ProductoId = productoId, Cantidad = cantidad });
                return RedirectToAction(nameof(Movimiento), new { productoId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var inventario = await _inventarioHttpServices.GetInventarioPorProductoAsync(productoId);
                return View("Movimiento", inventario);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AjustarStock(int productoId, int nuevoStock, int? nuevoStockMinimo)
        {
            try
            {
                await _inventarioHttpServices.AjustarInventarioAsync(new AjustarStockModel { ProductoId = productoId, NuevoStock = nuevoStock, NuevoStockMinimo = nuevoStockMinimo });
                return RedirectToAction(nameof(Movimiento), new { productoId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var inventario = await _inventarioHttpServices.GetInventarioPorProductoAsync(productoId);
                return View("Movimiento", inventario);
            }
        }

        // Alimenta el filtro por proveedor de la lista: solo los activos.
        private async Task CargarProveedores()
        {
            ViewBag.Proveedores = await _proveedorHttpServices.GetProveedoresActivosAsync();
        }
    }
}
