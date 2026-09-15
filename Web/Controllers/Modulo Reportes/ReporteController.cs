using Microsoft.AspNetCore.Mvc;
using System.Net;
using Web.Models.Modelo_Reportes.ReporteModels;
using Web.Services.Base;
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
            var esAdministrador = HttpContext.Session.GetString("ROL") == "Administrador";
            var model = new ReporteDashboardModel { Desde = desde, Hasta = hasta, EsAdministrador = esAdministrador };

            if (esAdministrador)
            {
                model.Ingresos = await _reporteHttpServices.GetIngresosTotalesAsync(desde, hasta);
                model.VentasPorFecha = await _reporteHttpServices.GetVentasPorFechaAsync(desde, hasta);
                model.TopProductos = await _reporteHttpServices.GetProductosMasVendidosAsync(desde, hasta, 5);

                var inventario = await _reporteHttpServices.GetInventarioActualAsync();
                model.TotalProductosInventario = inventario.Count;
                model.ProductosStockBajo = inventario.Count(i => i.StockBajo);
            }

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

        // ---------- Exportación a CSV (RF-3.5) ----------
        // El archivo se pide server-side, igual que la factura PDF: el JWT de sesión
        // viaja solo y la API nunca queda expuesta al navegador. Los filtros que llegan
        // acá son los mismos que la vista tiene en pantalla.

        [HttpGet]
        public async Task<ActionResult> ExportarVentasPorFecha(DateTime? desde, DateTime? hasta)
        {
            try
            {
                var (contenido, nombre) = await _reporteHttpServices.ExportarVentasPorFechaCsvAsync(desde, hasta);
                return File(contenido, ContenidoCsv, nombre ?? "ventas-por-fecha.csv");
            }
            catch (ApiHttpException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                // Se deja propagar para que HandleApiErrorsFilter cierre la sesión.
                throw;
            }
            catch (Exception ex)
            {
                return ManejarErrorExportacion(ex, nameof(VentasPorFecha), new { desde, hasta });
            }
        }

        [HttpGet]
        public async Task<ActionResult> ExportarVentasPorProducto(DateTime? desde, DateTime? hasta)
        {
            try
            {
                var (contenido, nombre) = await _reporteHttpServices.ExportarVentasPorProductoCsvAsync(desde, hasta);
                return File(contenido, ContenidoCsv, nombre ?? "ventas-por-producto.csv");
            }
            catch (ApiHttpException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw;
            }
            catch (Exception ex)
            {
                return ManejarErrorExportacion(ex, nameof(VentasPorProducto), new { desde, hasta });
            }
        }

        [HttpGet]
        public async Task<ActionResult> ExportarVentasPorVendedor(DateTime? desde, DateTime? hasta, int? usuarioId)
        {
            try
            {
                var (contenido, nombre) = await _reporteHttpServices.ExportarVentasPorVendedorCsvAsync(desde, hasta, usuarioId);
                return File(contenido, ContenidoCsv, nombre ?? "ventas-por-vendedor.csv");
            }
            catch (ApiHttpException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw;
            }
            catch (Exception ex)
            {
                return ManejarErrorExportacion(ex, nameof(VentasPorVendedor), new { desde, hasta, usuarioId });
            }
        }

        [HttpGet]
        public async Task<ActionResult> ExportarInventarioActual()
        {
            try
            {
                var (contenido, nombre) = await _reporteHttpServices.ExportarInventarioActualCsvAsync();
                return File(contenido, ContenidoCsv, nombre ?? "inventario-actual.csv");
            }
            catch (ApiHttpException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw;
            }
            catch (Exception ex)
            {
                return ManejarErrorExportacion(ex, nameof(InventarioActual), null);
            }
        }

        [HttpGet]
        public async Task<ActionResult> ExportarProductosMasVendidos(DateTime? desde, DateTime? hasta, int top = 10)
        {
            try
            {
                var (contenido, nombre) = await _reporteHttpServices.ExportarProductosMasVendidosCsvAsync(desde, hasta, top);
                return File(contenido, ContenidoCsv, nombre ?? "productos-mas-vendidos.csv");
            }
            catch (ApiHttpException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw;
            }
            catch (Exception ex)
            {
                return ManejarErrorExportacion(ex, nameof(ProductosMasVendidos), new { desde, hasta, top });
            }
        }

        [HttpGet]
        public async Task<ActionResult> ExportarIngresosTotales(DateTime? desde, DateTime? hasta)
        {
            try
            {
                var (contenido, nombre) = await _reporteHttpServices.ExportarIngresosTotalesCsvAsync(desde, hasta);
                return File(contenido, ContenidoCsv, nombre ?? "ingresos-totales.csv");
            }
            catch (ApiHttpException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw;
            }
            catch (Exception ex)
            {
                return ManejarErrorExportacion(ex, nameof(IngresosTotales), new { desde, hasta });
            }
        }

        private const string ContenidoCsv = "text/csv; charset=utf-8";

        private ActionResult ManejarError(Exception ex)
        {
            TempData["Error"] = ex.Message.Contains("Forbidden")
                ? "No tienes permisos para acceder a este reporte."
                : ex.Message;
            return RedirectToAction(nameof(Index));
        }

        // A diferencia de ManejarError, devuelve al mismo reporte con los mismos filtros:
        // si falla la descarga, mandar al usuario al índice le haría perder el filtro.
        private ActionResult ManejarErrorExportacion(Exception ex, string accion, object? filtros)
        {
            TempData["Error"] = ex.Message.Contains("Forbidden")
                ? "No tienes permisos para exportar este reporte."
                : ex.Message;
            return RedirectToAction(accion, filtros);
        }
    }
}
