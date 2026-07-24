using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Aplication.Dtos.PagoDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Result.ApiResponses;

namespace ProductApp.Api.Controllers.Modulo_Ventas
{
    [Route("api/[controller]")]
    [ApiController]
    public class PagoController : ControllerBase
    {
        private readonly IPagoServices _pagoServices;

        public PagoController(IPagoServices pagoServices)
        {
            _pagoServices = pagoServices;
        }

        [Authorize]
        [HttpPost("RegistrarPago")]
        public async Task<IActionResult> RegistrarPago(CreatePagoDto dto)
        {
            var result = await _pagoServices.RegistrarPagoAsync(dto);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<PagoResponseDto>.SuccessResponse(result.Data, result.Message));
        }

        [Authorize]
        [HttpGet("GetPagosByOrden/{ordenId}")]
        public async Task<IActionResult> GetPagosByOrden(int ordenId)
        {
            var result = await _pagoServices.ObtenerPagosPorOrdenAsync(ordenId);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<List<PagoResponseDto>>.SuccessResponse(result.Data, result.Message));
        }

        [Authorize]
        [HttpGet("GetSaldoPendiente/{ordenId}")]
        public async Task<IActionResult> GetSaldoPendiente(int ordenId)
        {
            var result = await _pagoServices.ObtenerSaldoPendienteAsync(ordenId);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<decimal>.SuccessResponse(result.Data, result.Message));
        }
    }
}
