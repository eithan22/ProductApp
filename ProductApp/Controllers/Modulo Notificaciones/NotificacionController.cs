using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Aplication.Dtos.Modulo_Notificaciones;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Result.ApiResponses;
using System.Security.Claims;

namespace ProductApp.Api.Controllers.Modulo_Notificaciones
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificacionController : ControllerBase
    {
        private readonly INotificacionServices _notificacionServices;

        public NotificacionController(INotificacionServices notificacionServices)
        {
            _notificacionServices = notificacionServices;
        }

        [Authorize]
        [HttpGet("GetResumen")]
        public async Task<IActionResult> GetResumen([FromQuery] int cantidad = 10)
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _notificacionServices.ObtenerResumenAsync(usuarioId, cantidad);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<NotificacionResumenDto>.SuccessResponse(result.Data, result.Message));
        }

        [Authorize]
        [HttpPatch("MarcarLeida/{id}")]
        public async Task<IActionResult> MarcarLeida(int id)
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _notificacionServices.MarcarComoLeidaAsync(id, usuarioId);

            if (!result.IsSuccess)
                return BadRequest(ApiResponse.FailureResponse(result.Message));

            return Ok(ApiResponse.SuccessResponse(result.Message));
        }

        [Authorize]
        [HttpPatch("MarcarTodasLeidas")]
        public async Task<IActionResult> MarcarTodasLeidas()
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _notificacionServices.MarcarTodasComoLeidasAsync(usuarioId);

            if (!result.IsSuccess)
                return BadRequest(ApiResponse.FailureResponse(result.Message));

            return Ok(ApiResponse.SuccessResponse(result.Message));
        }
    }
}
