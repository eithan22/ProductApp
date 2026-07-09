using Microsoft.AspNetCore.Mvc;
using ProductApp.Aplication.Common;
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

        public async Task<ActionResult> Index(int pageNumber = 1)
        {
            var result = await _inventarioHttpServices.GetAllInventariosAsync(pageNumber);
            var stockBajo = await _inventarioHttpServices.GetStockBajoAsync();
            ViewBag.SoloBajo = false;
            ViewBag.TotalBajo = stockBajo.Count;
            return View(result);
        }

        public async Task<ActionResult> StockBajo()
        {
            var result = await _inventarioHttpServices.GetStockBajoAsync();
            var paged = new PagedResult<InventarioModel> { Items = result, PageNumber = 1, PageSize = result.Count, TotalCount = result.Count };
            ViewBag.SoloBajo = true;
            ViewBag.TotalBajo = result.Count;
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
