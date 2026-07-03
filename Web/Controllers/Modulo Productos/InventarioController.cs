using Microsoft.AspNetCore.Mvc;
using Web.Models.Modelo_Productos.InventarioModels;
using Web.Services.Interfaces.ServicesHttp.Modulo_Productos;

namespace Web.Controllers.Modulo_Productos
{
    public class InventarioController : Controller
    {
        private readonly IInventarioHttpServices _inventarioHttpServices;

        public InventarioController(IInventarioHttpServices inventarioHttpServices)
        {
            _inventarioHttpServices = inventarioHttpServices;
        }

        public async Task<ActionResult> Index()
        {
            var result = await _inventarioHttpServices.GetAllInventariosAsync();
            ViewBag.SoloBajo = false;
            return View(result);
        }

        public async Task<ActionResult> StockBajo()
        {
            var result = await _inventarioHttpServices.GetStockBajoAsync();
            ViewBag.SoloBajo = true;
            return View("Index", result);
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
        public async Task<ActionResult> AjustarStock(int productoId, int nuevoStock)
        {
            try
            {
                await _inventarioHttpServices.AjustarInventarioAsync(new AjustarStockModel { ProductoId = productoId, NuevoStock = nuevoStock });
                return RedirectToAction(nameof(Movimiento), new { productoId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                var inventario = await _inventarioHttpServices.GetInventarioPorProductoAsync(productoId);
                return View("Movimiento", inventario);
            }
        }
    }
}
