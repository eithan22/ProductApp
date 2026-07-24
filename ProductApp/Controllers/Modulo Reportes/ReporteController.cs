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
    }
}
