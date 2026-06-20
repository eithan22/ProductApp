using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Aplication.Dtos.Modulo_Ventas.DetalleOrdenDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Result.ApiResponses;

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
            try
            {
                var result = await _detalleOrdenServices.AgregarProductoAsync(dto);

                if (!result.IsSuccess)
                    return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

                return Ok(ApiResponseT<OrdenDetalleResponseDto>.SuccessResponse(result.Data, result.Message));


            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseT<Object>.FailureResponse(ex.Message));
            }

        }

        //actualizar cantidad de producto en el detalle de orden

        [Authorize]
        [HttpPatch("UpdateDetalleOrden/{id}")]

        public async Task<IActionResult> UpdateDetalleOrden(int id, UpdateDetalleOrdenDto dto)
        {
            try
            {
                dto.id = id;
                var result = await _detalleOrdenServices.ActualizarDetalleOrden(id, dto);
                if (!result.IsSuccess)
                    return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

                return Ok(ApiResponseT<OrdenDetalleResponseDto>.SuccessResponse(result.Data, result.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseT<Object>.FailureResponse(ex.Message));
            }
        }



        //obtener detalle de orden por id

        [Authorize]
        [HttpGet("GetDetalleOrden/{id}")]

        public async Task<IActionResult> GetDetalleOrdenById(int id)
        {
            try
            {
                var result = await _detalleOrdenServices.GetOrdenDetalle(id);

                if (!result.IsSuccess)
                    return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

                return Ok(ApiResponseT<List<OrdenDetalleResponseDto>>.SuccessResponse(result.Data, result.Message));
            }
            catch (Exception ex)
            {   
                return BadRequest(ApiResponseT<Object>.FailureResponse(ex.Message));
            }

        }


        //eliminar producto del detalle de orden


        [Authorize]
        [HttpDelete("DeleteDetalleOrden/{id}")]

        public async Task<IActionResult> DeleteDetalleOrden(int id)
        {
            try
            {
               var result = await _detalleOrdenServices.EliminarProductoAsync(id);
                if (!result.IsSuccess)
                    return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

                return Ok(ApiResponse.SuccessResponse(result.Message));   
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseT<Object>.FailureResponse(ex.Message));
            }
        }




      



    }




}

