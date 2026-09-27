using Microsoft.AspNetCore.Mvc;
using System.Net;
using Web.Helpers;
using Web.Models.Modelo_Busqueda.BusquedaModels;
using Web.Services.Base;
using Web.Services.Interfaces.ServicesHttp.Modulo_Busqueda;

namespace Web.Controllers.Modulo_Busqueda
{
    // Controller sin vistas propias, igual que NotificacionController: solo le da de
    // comer al buscador del topbar. El JWT vive en la sesión del servidor, así que el
    // navegador nunca llama a la API directo: pide acá y esta acción reenvía con el token.
    public class BusquedaController : Controller
    {
        // El mismo mínimo que aplica el JS. Se repite acá porque la acción es una URL
        // pública del sitio: no puede depender de que el cliente se porte bien.
        private const int MinimoCaracteres = 2;

        private readonly IBusquedaHttpServices _busquedaHttpServices;
        private readonly ILogger<BusquedaController> _logger;

        public BusquedaController(IBusquedaHttpServices busquedaHttpServices,
            ILogger<BusquedaController> logger)
        {
            _busquedaHttpServices = busquedaHttpServices;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Buscar(string? texto)
        {
            var termino = (texto ?? string.Empty).Trim();

            if (termino.Length < MinimoCaracteres)
                return Json(SinResultados(termino, null));

            try
            {
                var resultado = await _busquedaHttpServices.BuscarGlobalAsync(termino);
                return Json(Proyectar(resultado));
            }
            catch (ApiHttpException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
            {
                // La sesión venció. El fetch no puede seguir el redirect que arma
                // HandleApiErrorsFilter, así que se le devuelve el destino al JS.
                HttpContext.Session.Clear();
                TempData["Aviso"] = "Tu sesión expiró. Iniciá sesión de nuevo.";
                return Json(new { redireccion = Url.Action("Login", "Auth") });
            }
            catch (Exception ex)
            {
                // Esta acción responde JSON: si la excepción subiera, el filtro global
                // devolvería un 302 y el dropdown recibiría HTML en vez de resultados.
                _logger.LogWarning(ex, "Falló la búsqueda global de «{Texto}»", termino);
                return Json(SinResultados(termino, ex.Message));
            }
        }

        // Las cuatro categorías se pintan con la misma fila, así que viajan con la misma
        // forma (nombre, meta, valor, sub, estado, url) y el JS tiene un solo render.
        // Los montos salen ya formateados porque MonedaHelper es la única fuente de
        // verdad de la moneda del sistema y vive en el servidor.
        private object Proyectar(BusquedaGlobalModel resultado)
        {
            var moneda = HttpContext.Session.GetString("MONEDA");

            var productos = resultado.Productos.Items.Select(p => new
            {
                nombre = p.Nombre,
                meta = string.IsNullOrWhiteSpace(p.Categoria) ? "Sin categoría" : p.Categoria,
                valor = MonedaHelper.Formatear(p.Precio, moneda),
                sub = p.StockActual.HasValue ? $"{p.StockActual} en stock" : "Sin inventario",
                estado = (string?)null,
                estadoClase = (string?)null,
                url = Url.Action("Details", "Producto", new { id = p.Id })
            });

            var clientes = resultado.Clientes.Items.Select(c => new
            {
                nombre = c.Nombre,
                // El artboard mostraba "N órdenes" a la derecha, pero la API no devuelve
                // ese contador: los datos de contacto ocupan la línea secundaria.
                meta = Unir(c.Cedula, c.Telefono, c.Correo),
                valor = (string?)null,
                sub = (string?)null,
                estado = (string?)null,
                estadoClase = (string?)null,
                url = Url.Action("Details", "Cliente", new { id = c.Id })
            });

            var ordenes = resultado.Ordenes.Items.Select(o => new
            {
                nombre = $"#{o.Id} · {o.NombreCliente}",
                meta = $"{o.Fecha:dd/MM/yyyy} · {o.CantidadProductos} {(o.CantidadProductos == 1 ? "producto" : "productos")}",
                valor = MonedaHelper.Formatear(o.Total, moneda),
                sub = o.SaldoPendiente > 0
                    ? $"saldo {MonedaHelper.Formatear(o.SaldoPendiente, moneda)}"
                    : "saldada",
                estado = o.Estado,
                estadoClase = $"estado-{o.Estado.ToLower()}",
                url = Url.Action("Details", "Orden", new { id = o.Id })
            });

            var proveedores = resultado.Proveedores.Items.Select(p => new
            {
                nombre = p.Nombre,
                meta = Unir(p.Telefono, p.Correo),
                valor = (string?)null,
                sub = (string?)null,
                estado = (string?)null,
                estadoClase = (string?)null,
                url = Url.Action("Details", "Proveedor", new { id = p.Id })
            });

            return new
            {
                texto = resultado.Texto,
                total = resultado.TotalCoincidencias,
                mensaje = (string?)null,
                grupos = new object[]
                {
                    new { clave = "producto",  titulo = "Productos",   total = resultado.Productos.TotalEncontrados,   hayMas = resultado.Productos.HayMasResultados,   items = productos },
                    new { clave = "cliente",   titulo = "Clientes",    total = resultado.Clientes.TotalEncontrados,    hayMas = resultado.Clientes.HayMasResultados,    items = clientes },
                    new { clave = "orden",     titulo = "Órdenes",     total = resultado.Ordenes.TotalEncontrados,     hayMas = resultado.Ordenes.HayMasResultados,     items = ordenes },
                    new { clave = "proveedor", titulo = "Proveedores", total = resultado.Proveedores.TotalEncontrados, hayMas = resultado.Proveedores.HayMasResultados, items = proveedores }
                }
            };
        }

        private static object SinResultados(string texto, string? mensaje) => new
        {
            texto,
            total = 0,
            mensaje,
            grupos = Array.Empty<object>()
        };

        // Los datos de contacto son opcionales en el dominio: se arma la línea solo
        // con los que vinieron, para no dejar separadores sueltos.
        private static string Unir(params string?[] partes) =>
            string.Join(" · ", partes.Where(p => !string.IsNullOrWhiteSpace(p)));
    }
}
