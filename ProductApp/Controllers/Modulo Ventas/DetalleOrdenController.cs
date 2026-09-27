using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Aplication.Dtos.Modulo_Ventas.DetalleOrdenDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Result.ApiResponses;
using System.Security.Claims;

namespace ProductApp.Api.Controllers.Modulo_Ventas
{
    [Route("api/[controller]")]
    [ApiController]
    public class DetalleOrdenController : ControllerBase
    {
        private readonly IDetalleOrdenServices _detalleOrdenServices;

        public DetalleOrdenController(IDetalleOrdenServices detalleOrdenServices) 
        {
            _detalleOrdenServices = detalleOrdenServices;

        
        }


        //agregar producto al detalle de orden
        [Authorize]
        [HttpPost("CreateDetalleOrden")]

        
        public async Task<IActionResult> CreateDetalleOrden(CreateDetalleOrdenDto  dto)
        {
            var usuarioSolicitanteId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var esAdministrador = User.IsInRole("Administrador");
            var result = await _detalleOrdenServices.AgregarProductoAsync(dto, usuarioSolicitanteId, esAdministrador);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<OrdenDetalleResponseDto>.SuccessResponse(result.Data, result.Message));
        }

        //actualizar cantidad de producto en el detalle de orden

        [Authorize]
        [HttpPatch("UpdateDetalleOrden/{id}")]

        public async Task<IActionResult> UpdateDetalleOrden(int id, UpdateDetalleOrdenDto dto)
        {
            dto.id = id;
            var usuarioSolicitanteId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var esAdministrador = User.IsInRole("Administrador");
            var result = await _detalleOrdenServices.ActualizarDetalleOrden(id, dto, usuarioSolicitanteId, esAdministrador);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<OrdenDetalleResponseDto>.SuccessResponse(result.Data, result.Message));
        }



        //obtener detalle de orden por id

        [Authorize]
        [HttpGet("GetDetalleOrden/{id}")]

        public async Task<IActionResult> GetDetalleOrdenById(int id)
        {
            var result = await _detalleOrdenServices.GetOrdenDetalle(id);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<List<OrdenDetalleResponseDto>>.SuccessResponse(result.Data, result.Message));
        }


        //eliminar producto del detalle de orden


        [Authorize]
        [HttpDelete("DeleteDetalleOrden/{id}")]

        public async Task<IActionResult> DeleteDetalleOrden(int id)
        {
            var usuarioSolicitanteId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var esAdministrador = User.IsInRole("Administrador");
            var result = await _detalleOrdenServices.EliminarProductoAsync(id, usuarioSolicitanteId, esAdministrador);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponse.SuccessResponse(result.Message));
        }




      



    }




}

