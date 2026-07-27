using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Aplication.Dtos.Modulo_Ventas.OrdenDto;
using ProductApp.Aplication.Dtos.OrdenDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Result.ApiResponses;
using ProductApp.Domian.Common.Enums.EnumsOrden;
using System.Security.Claims;

namespace ProductApp.Api.Controllers.Modulo_Ventas
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdenController : ControllerBase
    {
        private readonly IOrdenServices _ordenServices;

        public OrdenController(IOrdenServices ordenServices) 
        {
            _ordenServices = ordenServices;

        
        }

        //crear orden y listar ordenes por cliente

        [Authorize]
        [HttpPost("CreateOrden")]

        public async Task<IActionResult> CreateOrden(CreateOrdenDto dto)
        {
            //agregando el usuario id al dto para crear la orden, el usuario id se obtiene del token de autenticacion
            var usuarioId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

            var result = await _ordenServices.CrearOrden(dto, usuarioId);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<OrdenResponseDto>.SuccessResponse(result.Data, result.Message));
        }

        //

        [Authorize]
        [HttpGet("GetAllOrdenes")]

        public async Task<IActionResult> GetAllOrdenes([FromQuery] string? estado = null)
        {
            if (!TryParseEstadoFiltro(estado, out var estadoFiltro, out var error))
                return error!;

            var result = await _ordenServices.GetAllOrdenes(estadoFiltro);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<List<OrdenResponseDto>>.SuccessResponse(result.Data, result.Message));
        }





        //Cancelar orden por id, solo si la orden esta en estado pendiente

        [Authorize]
        [HttpPatch("CancelarOrden/{id}")]

        public async Task<IActionResult> CancelarOrden(int id)
        {
            var usuarioSolicitanteId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _ordenServices.CancelarOrden(id, usuarioSolicitanteId);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponse.SuccessResponse(result.Message));
        }

        //Confirmar orden por id (pendiente -> procesada), solo si tiene productos

        [Authorize]
        [HttpPatch("ConfirmarOrden/{id}")]

        public async Task<IActionResult> ConfirmarOrden(int id)
        {
            var usuarioSolicitanteId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _ordenServices.ConfirmarOrden(id, usuarioSolicitanteId);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponse.SuccessResponse(result.Message));
        }


        //

        [Authorize]
        [HttpPatch("CambiarEstadoOrden")]

        public async Task<IActionResult> CambiarEstadoOrden(CambiarEstadoOrdenDto dto)
        {
            var result = await _ordenServices.CambiarEstadoOrden(dto);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponse.SuccessResponse(result.Message));
        }


        [Authorize]
        [HttpGet("GetOrdenById/{id}")]
        public async Task<IActionResult> GetOrdenById(int id)
        {
            var result = await _ordenServices.GetOrdenByIdAsync(id);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<OrdenResponseDto>.SuccessResponse(result.Data, result.Message));
        }

        [Authorize]
        [HttpGet("GetOrdenByClientes/{id}")]

        public async Task<IActionResult> GetOrdenByClientes(int id, [FromQuery] string? estado = null)
        {
            if (!TryParseEstadoFiltro(estado, out var estadoFiltro, out var error))
                return error!;

            var result = await _ordenServices.ConsultarOrdenesPorCliente(id, estadoFiltro);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<List<OrdenResponseDto>>.SuccessResponse(result.Data, result.Message));
        }



        [Authorize]
        [HttpGet("GetOrdenesByUsuario/{usuarioId}")]
        public async Task<IActionResult> GetOrdenesByUsuario(int usuarioId)
        {
            var result = await _ordenServices.GetOrdenesByUsuarioAsync(usuarioId);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<List<OrdenResponseDto>>.SuccessResponse(result.Data, result.Message));
        }

        [Authorize]
        [HttpGet("GetOrdenByFecha/{fecha}")]

        public async Task<IActionResult> GetOrdenByFecha(DateTime fecha, [FromQuery] string? estado = null)
        {
            if (!TryParseEstadoFiltro(estado, out var estadoFiltro, out var error))
                return error!;

            var result = await _ordenServices.ConsultarOrdenesPorFecha(fecha, estadoFiltro);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<List<OrdenResponseDto>>.SuccessResponse(result.Data, result.Message));
        }

        private bool TryParseEstadoFiltro(string? estado, out EstadoOrden? estadoFiltro, out IActionResult? error)
        {
            estadoFiltro = null;
            error = null;

            if (string.IsNullOrWhiteSpace(estado))
                return true;

            if (!Enum.TryParse<EstadoOrden>(estado, true, out var parsed))
            {
                error = BadRequest(ApiResponseT<Object>.FailureResponse("Estado inválido"));
                return false;
            }

            estadoFiltro = parsed;
            return true;
        }

    }




}

