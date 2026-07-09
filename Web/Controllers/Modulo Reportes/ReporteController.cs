using Microsoft.AspNetCore.Mvc;
using Web.Models.Modelo_Reportes.ReporteModels;
using Web.Services.Interfaces.ServicesHttp.Modulo_Reportes;

namespace Web.Controllers.Modulo_Reportes
{
    public class ReporteController : Controller
    {
        private readonly IReporteHttpServices _reporteHttpServices;

        public ReporteController(IReporteHttpServices reporteHttpServices)
        {
            _reporteHttpServices = reporteHttpServices;
        }

        public async Task<ActionResult> Index()
        {
            var hasta = DateTime.Today;
            var desde = hasta.AddDays(-30);
            var model = new ReporteDashboardModel { Desde = desde, Hasta = hasta };

            try { model.Ingresos = await _reporteHttpServices.GetIngresosTotalesAsync(desde, hasta); }
            catch { }

            try { model.VentasPorFecha = await _reporteHttpServices.GetVentasPorFechaAsync(desde, hasta); }
            catch { }

            try { model.TopProductos = await _reporteHttpServices.GetProductosMasVendidosAsync(desde, hasta, 5); }
            catch { }

            try
            {
                var inventario = await _reporteHttpServices.GetInventarioActualAsync();
                model.TotalProductosInventario = inventario.Count;
                model.ProductosStockBajo = inventario.Count(i => i.StockBajo);
            }
            catch { }

            return View(model);
        }

        public async Task<ActionResult> VentasPorFecha(DateTime? desde, DateTime? hasta)
        {
            try
            {
                var result = await _reporteHttpServices.GetVentasPorFechaAsync(desde, hasta);
                ViewBag.Desde = desde;
                ViewBag.Hasta = hasta;
                return View(result);
            }
            catch (Exception ex)
            {
                return ManejarError(ex);
            }
        }

        public async Task<ActionResult> VentasPorProducto(DateTime? desde, DateTime? hasta)
        {
            try
            {
                var result = await _reporteHttpServices.GetVentasPorProductoAsync(desde, hasta);
                ViewBag.Desde = desde;
                ViewBag.Hasta = hasta;
                return View(result);
            }
            catch (Exception ex)
            {
                return ManejarError(ex);
            }
        }

        public async Task<ActionResult> VentasPorVendedor(DateTime? desde, DateTime? hasta, int? usuarioId)
        {
            try
            {
                var result = await _reporteHttpServices.GetVentasPorVendedorAsync(desde, hasta, usuarioId);
                ViewBag.Desde = desde;
                ViewBag.Hasta = hasta;
                ViewBag.UsuarioId = usuarioId;
                return View(result);
            }
            catch (Exception ex)
            {
                return ManejarError(ex);
            }
        }

        public async Task<ActionResult> InventarioActual()
        {
            try
            {
                var result = await _reporteHttpServices.GetInventarioActualAsync();
                return View(result);
            }
            catch (Exception ex)
            {
                return ManejarError(ex);
            }
        }

        public async Task<ActionResult> ProductosMasVendidos(DateTime? desde, DateTime? hasta, int top = 10)
        {
            try
            {
                var result = await _reporteHttpServices.GetProductosMasVendidosAsync(desde, hasta, top);
                ViewBag.Desde = desde;
                ViewBag.Hasta = hasta;
                ViewBag.Top = top;
                return View(result);
            }
            catch (Exception ex)
            {
                return ManejarError(ex);
            }
        }

        public async Task<ActionResult> IngresosTotales(DateTime? desde, DateTime? hasta)
        {
            try
            {
                var result = await _reporteHttpServices.GetIngresosTotalesAsync(desde, hasta);
                return View(result);
            }
            catch (Exception ex)
            {
                return ManejarError(ex);
            }
        }

        private ActionResult ManejarError(Exception ex)
        {
            TempData["Error"] = ex.Message.Contains("Forbidden")
                ? "No tienes permisos para acceder a este reporte."
                : ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }
}
