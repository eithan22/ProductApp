using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Aplication.Dtos.Modulo_Configuracion;
using ProductApp.Aplication.Interface.Servicios.Modulo_Configuracion;
using ProductApp.Aplication.Result.ApiResponses;

namespace ProductApp.Api.Controllers.Modulo_Configuracion
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfiguracionController : ControllerBase
    {
        private readonly IConfiguracionSistemaService _configuracionSistemaService;

        public ConfiguracionController(IConfiguracionSistemaService configuracionSistemaService)
        {
            _configuracionSistemaService = configuracionSistemaService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Obtener()
        {
            try
            {
                var result = await _configuracionSistemaService.ObtenerAsync();
                if (!result.IsSuccess)
                    return BadRequest(ApiResponseT<object>.FailureResponse(result.Message));

                return Ok(ApiResponseT<ConfiguracionSistemaDto>.SuccessResponse(result.Data, result.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseT<object>.FailureResponse(ex.Message));
            }
        }

        [Authorize(Roles = "Administrador")]
        [HttpPut]
        public async Task<IActionResult> Actualizar(ActualizarConfiguracionSistemaDto dto)
        {
            try
            {
                var result = await _configuracionSistemaService.ActualizarAsync(dto);
                if (!result.IsSuccess)
                    return BadRequest(ApiResponseT<object>.FailureResponse(result.Message));

                return Ok(ApiResponseT<ConfiguracionSistemaDto>.SuccessResponse(result.Data, result.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseT<object>.FailureResponse(ex.Message));
            }
        }
    }
}
