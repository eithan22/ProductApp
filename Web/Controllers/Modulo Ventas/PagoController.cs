using Microsoft.AspNetCore.Mvc;
using ProductApp.Aplication.Common;
using Web.Models.Modelo_Ventas.PagoModels;
using Web.Services.Interfaces.ServicesHttp.Modulo_Ventas;

namespace Web.Controllers.Modulo_Ventas
{
    public class PagoController : Controller
    {
        private readonly IPagoHttpServices _pagoHttpServices;
        private readonly IOrdenHttpServices _ordenHttpServices;

        public PagoController(
            IPagoHttpServices pagoHttpServices,
            IOrdenHttpServices ordenHttpServices)
        {
            _pagoHttpServices = pagoHttpServices;
            _ordenHttpServices = ordenHttpServices;
        }

        public async Task<ActionResult> Index(
            int? ordenId = null,
            DateTime? desde = null,
            DateTime? hasta = null,
            string? metodoPago = null,
            int pageNumber = 1,
            int pageSize = PaginacionDefaults.PageSizeDefault)
        {
            var listado = await _pagoHttpServices.GetPagosAsync(
                ordenId, desde, hasta, metodoPago, pageNumber, pageSize);

            var vm = new PagoListadoViewModel
            {
                Pagos = listado.Pagos.Items,
                TotalRecibido = listado.TotalRecibido,
                CantidadPagos = listado.CantidadPagos,
                OrdenesSaldadas = listado.OrdenesSaldadas,
                PorMetodo = listado.PorMetodo,
                PageNumber = listado.Pagos.PageNumber,
                TotalPages = listado.Pagos.TotalPages,
                OrdenId = ordenId,
                Desde = desde,
                Hasta = hasta,
                MetodoPago = metodoPago
            };

            return View(vm);
        }

        // El botón "Registrar pago" de la pantalla de Pagos no tiene una orden en
        // contexto, así que primero se elige entre las órdenes que todavía admiten
        // cobro y el formulario se despliega sobre la que se seleccione.
        public async Task<ActionResult> Registrar()
        {
            // Mismo servicio y mismo filtro que OrdenController.Index: una sola
            // llamada, que del lado de la API ya excluye las canceladas.
            var ordenes = await _ordenHttpServices.GetOrdenesAsync();

            // Misma regla de cobrabilidad que Orden/Details: solo Pendiente y
            // Procesada admiten pago, y solo mientras quede saldo.
            var candidatas = ordenes
                .Where(o => o.Estado == "Pendiente" || o.Estado == "Procesada")
                .OrderByDescending(o => o.Fecha)
                .ToList();

            // El saldo se pide orden por orden porque OrdenModel no lo trae. El
            // conjunto está acotado a lo que falta cobrar, no al histórico. No se
            // paraleliza a propósito: BaseHttpServices lee el token desde ISession,
            // que no es thread-safe.
            var cobrables = new List<OrdenCobrableModel>();
            foreach (var orden in candidatas)
            {
                var saldo = await _pagoHttpServices.GetSaldoPendienteAsync(orden.Id);
                if (saldo <= 0) { continue; }

                cobrables.Add(new OrdenCobrableModel
                {
                    Id = orden.Id,
                    NombreCliente = orden.NombreCliente,
                    Total = orden.Total,
                    SaldoPendiente = saldo,
                    Estado = orden.Estado,
                    Fecha = orden.Fecha
                });
            }

            return View(cobrables);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Registrar(int ordenId, decimal monto, string metodoPago)
        {
            try
            {
                await _pagoHttpServices.RegistrarPagoAsync(new CreatePagoModel
                {
                    OrdenId = ordenId,
                    Monto = monto,
                    MetodoPago = metodoPago
                });

                TempData["Mensaje"] = $"Pago registrado en la orden #{ordenId}.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Registrar));
            }
        }
    }
}
