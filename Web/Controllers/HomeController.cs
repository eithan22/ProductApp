using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net;
using Web.Models;
using Web.Models.Modelo_Reportes.ReporteModels;
using Web.Services.Base;
using Web.Services.Interfaces.ServicesHttp;
using Web.Services.Interfaces.ServicesHttp.Modulo_Productos;
using Web.Services.Interfaces.ServicesHttp.Modulo_Reportes;
using Web.Services.Interfaces.ServicesHttp.Modulo_Ventas;

namespace Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IReporteHttpServices _reporteHttpServices;
        private readonly IOrdenHttpServices _ordenHttpServices;
        private readonly IInventarioHttpServices _inventarioHttpServices;
        private readonly IClienteHttpServices _clienteHttpServices;

        public HomeController(
            ILogger<HomeController> logger,
            IReporteHttpServices reporteHttpServices,
            IOrdenHttpServices ordenHttpServices,
            IInventarioHttpServices inventarioHttpServices,
            IClienteHttpServices clienteHttpServices)
        {
            _logger = logger;
            _reporteHttpServices = reporteHttpServices;
            _ordenHttpServices = ordenHttpServices;
            _inventarioHttpServices = inventarioHttpServices;
            _clienteHttpServices = clienteHttpServices;
        }

        public async Task<IActionResult> Index()
        {
            var rol = HttpContext.Session.GetString("ROL") ?? string.Empty;

            var model = new HomeDashboardModel
            {
                Nombre = HttpContext.Session.GetString("NOMBRE") ?? string.Empty,
                Rol = rol,
                EsAdministrador = rol == "Administrador"
            };

            // Una sola llamada alimenta el KPI de pendientes y la lista de recientes.
            // GetAllOrdenes ya excluye las canceladas del lado de la API.
            var ordenes = await CargarBloqueAsync(
                () => _ordenHttpServices.GetOrdenesAsync(), "órdenes", model);

            if (ordenes is not null)
            {
                model.OrdenesPendientes = ordenes.Count(o => o.Estado == "Pendiente");
                model.OrdenesRecientes = ordenes
                    .OrderByDescending(o => o.Fecha)
                    .Take(6)
                    .ToList();
            }

            // Ídem: una llamada para el KPI y para las alertas.
            var stockBajo = await CargarBloqueAsync(
                () => _inventarioHttpServices.GetStockBajoAsync(), "inventario", model);

            if (stockBajo is not null)
            {
                model.ProductosStockBajo = stockBajo.Count;
                model.AlertasStock = stockBajo
                    .OrderBy(i => i.StockActual)
                    .Take(5)
                    .ToList();
            }

            var clientes = await CargarBloqueAsync(
                () => _clienteHttpServices.GetClientesAsync(), "clientes", model);

            if (clientes is not null)
            {
                model.ClientesActivos = clientes.TotalCount;
            }

            // Reportes es solo-Administrador en la API: ni lo intentamos para otros roles.
            if (model.EsAdministrador)
            {
                var hoy = DateTime.Today;

                var ingresos = await CargarBloqueAsync(
                    () => _reporteHttpServices.GetIngresosTotalesAsync(hoy, hoy), "ingresos", model);

                if (ingresos is not null)
                {
                    model.IngresosHoy = ingresos.Total;
                }

                var desde = hoy.AddDays(-6);
                var ventas = await CargarBloqueAsync(
                    () => _reporteHttpServices.GetVentasPorFechaAsync(desde, hoy), "ventas de la semana", model);

                if (ventas is not null)
                {
                    model.VentasSemana = CompletarSemana(ventas, desde);
                }
            }

            return View(model);
        }

        /// <summary>
        /// Ejecuta una llamada a la API aislando su error: si un bloque falla,
        /// el resto del panel se sigue mostrando. El 401 es la única excepción:
        /// se deja propagar para que HandleApiErrorsFilter cierre la sesión.
        /// </summary>
        private async Task<T?> CargarBloqueAsync<T>(
            Func<Task<T>> cargar, string bloque, HomeDashboardModel model) where T : class
        {
            try
            {
                return await cargar();
            }
            catch (ApiHttpException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo cargar el bloque '{Bloque}' del panel de inicio", bloque);
                model.HuboErrores = true;
                return null;
            }
        }

        /// <summary>
        /// La API solo devuelve los días que tuvieron ventas. Para el gráfico
        /// completamos los 7 días; un día ausente significa 0 ventas de verdad.
        /// </summary>
        private static List<VentaPorFechaModel> CompletarSemana(
            List<VentaPorFechaModel> ventas, DateTime desde)
        {
            var porDia = ventas
                .GroupBy(v => v.Fecha.Date)
                .ToDictionary(g => g.Key, g => g.First());

            return Enumerable.Range(0, 7)
                .Select(i => desde.AddDays(i).Date)
                .Select(dia => porDia.TryGetValue(dia, out var v)
                    ? v
                    : new VentaPorFechaModel { Fecha = dia, CantidadOrdenes = 0, Total = 0 })
                .ToList();
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
            => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
