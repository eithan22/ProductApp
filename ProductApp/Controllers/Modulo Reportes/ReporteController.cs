using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Aplication.Dtos.ReporteDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Result.ApiResponses;
using System.Security.Claims;

namespace ProductApp.Api.Controllers.Modulo_Reportes
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReporteController : ControllerBase
    {
        private readonly IReporteServices _reporteServices;

        public ReporteController(IReporteServices reporteServices)
        {
            _reporteServices = reporteServices;
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet("VentasPorFecha")]
        public async Task<IActionResult> VentasPorFecha([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            var result = await _reporteServices.ObtenerVentasPorFechaAsync(desde, hasta);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<List<VentaPorFechaDto>>.SuccessResponse(result.Data, result.Message));
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet("VentasPorProducto")]
        public async Task<IActionResult> VentasPorProducto([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            var result = await _reporteServices.ObtenerVentasPorProductoAsync(desde, hasta);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<List<VentaPorProductoDto>>.SuccessResponse(result.Data, result.Message));
        }

        [Authorize]
        [HttpGet("VentasPorVendedor")]
        public async Task<IActionResult> VentasPorVendedor([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta, [FromQuery] int? usuarioId)
        {
            var usuarioAutenticadoId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var esAdministrador = User.IsInRole("Administrador");

            var result = await _reporteServices.ObtenerVentasPorVendedorAsync(desde, hasta, usuarioId, usuarioAutenticadoId, esAdministrador);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<List<VentaPorVendedorDto>>.SuccessResponse(result.Data, result.Message));
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet("InventarioActual")]
        public async Task<IActionResult> InventarioActual()
        {
            var result = await _reporteServices.ObtenerInventarioActualAsync();
            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<List<InventarioActualDto>>.SuccessResponse(result.Data, result.Message));
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet("ProductosMasVendidos")]
        public async Task<IActionResult> ProductosMasVendidos([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta, [FromQuery] int top = 10)
        {
            var result = await _reporteServices.ObtenerProductosMasVendidosAsync(desde, hasta, top);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<List<ProductoMasVendidoDto>>.SuccessResponse(result.Data, result.Message));
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet("IngresosTotales")]
        public async Task<IActionResult> IngresosTotales([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            var result = await _reporteServices.ObtenerIngresosTotalesAsync(desde, hasta);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<IngresosTotalesDto>.SuccessResponse(result.Data, result.Message));
        }

        // ---------- Exportación a CSV (RF-3.5, ver docs/06-openapi.yaml) ----------
        // Cada endpoint repite exactamente el [Authorize] de su reporte: quien puede ver
        // el reporte en pantalla puede exportarlo, ni más ni menos.

        [Authorize(Roles = "Administrador")]
        [HttpGet("VentasPorFecha/Exportar")]
        public async Task<IActionResult> ExportarVentasPorFecha([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            var result = await _reporteServices.ExportarVentasPorFechaCsvAsync(desde, hasta);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<object>.FailureResponse(result.Message));

            return File(result.Data!, ContenidoCsv, NombreArchivo("ventas-por-fecha", desde, hasta));
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet("VentasPorProducto/Exportar")]
        public async Task<IActionResult> ExportarVentasPorProducto([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            var result = await _reporteServices.ExportarVentasPorProductoCsvAsync(desde, hasta);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<object>.FailureResponse(result.Message));

            return File(result.Data!, ContenidoCsv, NombreArchivo("ventas-por-producto", desde, hasta));
        }

        [Authorize]
        [HttpGet("VentasPorVendedor/Exportar")]
        public async Task<IActionResult> ExportarVentasPorVendedor([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta, [FromQuery] int? usuarioId)
        {
            var usuarioAutenticadoId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var esAdministrador = User.IsInRole("Administrador");

            var result = await _reporteServices.ExportarVentasPorVendedorCsvAsync(desde, hasta, usuarioId, usuarioAutenticadoId, esAdministrador);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<object>.FailureResponse(result.Message));

            return File(result.Data!, ContenidoCsv, NombreArchivo("ventas-por-vendedor", desde, hasta));
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet("InventarioActual/Exportar")]
        public async Task<IActionResult> ExportarInventarioActual()
        {
            var result = await _reporteServices.ExportarInventarioActualCsvAsync();
            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<object>.FailureResponse(result.Message));

            return File(result.Data!, ContenidoCsv, NombreArchivo("inventario-actual", null, null));
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet("ProductosMasVendidos/Exportar")]
        public async Task<IActionResult> ExportarProductosMasVendidos([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta, [FromQuery] int top = 10)
        {
            var result = await _reporteServices.ExportarProductosMasVendidosCsvAsync(desde, hasta, top);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<object>.FailureResponse(result.Message));

            return File(result.Data!, ContenidoCsv, NombreArchivo("productos-mas-vendidos", desde, hasta));
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet("IngresosTotales/Exportar")]
        public async Task<IActionResult> ExportarIngresosTotales([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            var result = await _reporteServices.ExportarIngresosTotalesCsvAsync(desde, hasta);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<object>.FailureResponse(result.Message));

            return File(result.Data!, ContenidoCsv, NombreArchivo("ingresos-totales", desde, hasta));
        }

        // Se declara el charset además del BOM: hay clientes que ignoran el BOM si el
        // encabezado no dice utf-8.
        private const string ContenidoCsv = "text/csv; charset=utf-8";

        // El nombre lo arma la API y viaja en Content-Disposition; la capa Web lo lee de
        // ahí en vez de reconstruirlo, para que no existan dos versiones del mismo nombre.
        private static string NombreArchivo(string reporte, DateTime? desde, DateTime? hasta)
        {
            var sufijo = desde.HasValue && hasta.HasValue
                ? $"{desde.Value:yyyy-MM-dd}-a-{hasta.Value:yyyy-MM-dd}"
                : $"{DateTime.Now:yyyy-MM-dd}";

            return $"{reporte}-{sufijo}.csv";
        }
    }
}
