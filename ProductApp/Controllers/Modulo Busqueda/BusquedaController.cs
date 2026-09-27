using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Aplication.Dtos.Modulo_Busqueda.BusquedaDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Result.ApiResponses;

namespace ProductApp.Api.Controllers.Modulo_Busqueda
{
    [Route("api/[controller]")]
    [ApiController]
    public class BusquedaController : ControllerBase
    {
        private readonly IBusquedaServices _busquedaServices;

        public BusquedaController(IBusquedaServices busquedaServices)
        {
            _busquedaServices = busquedaServices;
        }

        // [Authorize] plano, sin roles: RF-3.8.3 pide que Administrador y Vendedor vean
        // exactamente lo mismo que ya ven en cada módulo por separado.
        [Authorize]
        [HttpGet("GetBuscarGlobal")]
        public async Task<IActionResult> GetBuscarGlobal([FromQuery] string texto)
        {
            var result = await _busquedaServices.BuscarGlobalAsync(texto);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<BusquedaGlobalResponseDto>.SuccessResponse(result.Data, result.Message));
        }
    }
}
