using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Aplication.Dtos.CategoriaDto;
using ProductApp.Aplication.Dtos.Modulo_Productos.InventarioDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Result.ApiResponses;

namespace ProductApp.Api.Controllers.Modulo_Productos
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventarioController : ControllerBase
    {
        private readonly IInventarioServices _inventarioService;

        public InventarioController(IInventarioServices inventarioService)
        {
            _inventarioService = inventarioService;
        }


        [HttpGet("GetInventarioPorProducto/{productoId}")]
        public async Task<IActionResult> GetInventario(int productoId)
        {
            try
            {
                var result = await _inventarioService.ObtenerInventarioAsync(productoId);


                if (!result.IsSuccess)
                    return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));


                return Ok(ApiResponseT<InventarioResponseDto>.SuccessResponse(result.Data, result.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseT<Object>.FailureResponse(ex.Message));

            }

        }




        [HttpGet("GetAllInventarios")]
        public async Task<IActionResult> GetAllInventarios()
        {
            try
            {
                var result = await _inventarioService.ObtenerTodosInventariosAsync();


                if (!result.IsSuccess)
                    return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));


                return Ok(ApiResponseT<List<InventarioResponseDto>>.SuccessResponse(result.Data, result.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseT<Object>.FailureResponse(ex.Message));

            }


        }


        [HttpGet("GetStockBajo")]
        public async Task<IActionResult> GetStockBajo()
        {
            try
            {
                var result = await _inventarioService.ObtenerStockBajoAsync();


                if (!result.IsSuccess)
                    return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));


                return Ok(ApiResponseT<List<InventarioResponseDto>>.SuccessResponse(result.Data, result.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseT<Object>.FailureResponse(ex.Message));

            }


        }


        [Authorize]
        [HttpPost("AgregarStock")]

        public async Task<IActionResult> AgregarStockAsync( MovimientoStockDto dto)
        {
            try
            {
                var result = await _inventarioService.AgregarStockAsync(dto);

                if (!result.IsSuccess)
                    return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

                return Ok(ApiResponseT<InventarioResponseDto>.SuccessResponse(result.Data, result.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseT<Object>.FailureResponse(ex.Message));
            }
        }



        [Authorize]
        [HttpPost("DescontarStock/{productoId}")]

        public async Task<IActionResult> DescontarStockAsync(int productoId, MovimientoStockDto dto)
        {
            try
            {
                dto.ProductoId = productoId;
                var result = await _inventarioService.DescontarStockAsync(dto);

                if (!result.IsSuccess)
                    return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

                return Ok(ApiResponseT<InventarioResponseDto>.SuccessResponse(result.Data, result.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseT<Object>.FailureResponse(ex.Message));
            }
        }




        [Authorize]
        [HttpPut("AjustarInventario")]

        public async Task<IActionResult> AjustarInventario(int productoId, AjustarStockDto dto)
        {
            try { 
          
                dto.ProductoId = productoId;

                var result = await _inventarioService.AjustarStockAsync(dto);

                if (!result.IsSuccess)
                    return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

                return Ok(ApiResponseT<InventarioResponseDto>.SuccessResponse(result.Data, result.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseT<Object>.FailureResponse(ex.Message));
            }
        }





    }

    }