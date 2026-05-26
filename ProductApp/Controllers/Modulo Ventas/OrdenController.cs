using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Aplication.Dtos.CategoriaDto;
using ProductApp.Aplication.Dtos.ClienteDto;
using ProductApp.Aplication.Dtos.Modulo_Ventas.OrdenDto;
using ProductApp.Aplication.Dtos.OrdenDto;
using ProductApp.Aplication.Dtos.ProductoDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Result.ApiResponses;
using ProductApp.Aplication.Services;

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
            try
            {
                var result = await _ordenServices.CrearOrden(dto);
                if (!result.IsSuccess)
                    return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

                return Ok(ApiResponseT<OrdenResponseDto>.SuccessResponse(result.Data, result.Message));

            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseT<Object>.FailureResponse(ex.Message));
            }

        }



        [Authorize]
        [HttpGet("GetAllOrdenes/{id}")]

        public async Task<IActionResult> GetAllOrdenes()
        {
            try
            {
                var result = await _ordenServices.GetAllOrdenes();
                if (!result.IsSuccess)
                    return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

                return Ok(ApiResponseT<List<OrdenResponseDto>>.SuccessResponse(result.Data, result.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseT<Object>.FailureResponse(ex.Message));
            }
        }





        //Cancelar orden por id, solo si la orden esta en estado pendiente

        [Authorize]
        [HttpPatch("CancelarOrden/{id}")]

        public async Task<IActionResult> CancelarOrden(int id)
        {
            try
            {
                var result = await _ordenServices.CancelarOrden(id);
                if (!result.IsSuccess)
                    return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));


                return Ok(ApiResponse.SuccessResponse(result.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseT<Object>.FailureResponse(ex.Message));

            }

        }


        //

        [Authorize]
        [HttpPatch("CambiarEstadoOrden")]

        public async Task<IActionResult> CambiarEstadoOrden(CambiarEstadoOrdenDto dto)
        {
            try
            {
                var result = await _ordenServices.CambiarEstadoOrden(dto);
                if (!result.IsSuccess)
                    return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

                return Ok(ApiResponse.SuccessResponse(result.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseT<Object>.FailureResponse(ex.Message));
            }

        }


        //obtener ordenes por clientes

        [Authorize]
        [HttpGet("GetOrdenByClientes/{id}")]

        public async Task<IActionResult> GetOrdenByClientes(int id)
        {
            try
            {
                var result = await _ordenServices.ConsultarOrdenesPorCliente(id);
                if (!result.IsSuccess)
                    return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

                return Ok(ApiResponseT<List<OrdenResponseDto>>.SuccessResponse(result.Data, result.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseT<Object>.FailureResponse(ex.Message));
            }
        }


        //obtener ordenes por fecha

        [Authorize]
        [HttpGet("GetOrdenByFecha/{fecha}")]

        public async Task<IActionResult> GetOrdenByFecha(DateTime fecha)
        {
            try
            {
                var result = await _ordenServices.ConsultarOrdenesPorFecha(fecha);
                if (!result.IsSuccess)
                    return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

                return Ok(ApiResponseT<List<OrdenResponseDto>>.SuccessResponse(result.Data, result.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseT<Object>.FailureResponse(ex.Message));
            }
        }



       

    }




}

