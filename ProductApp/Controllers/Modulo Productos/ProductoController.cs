using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Aplication.Dtos.CategoriaDto;
using ProductApp.Aplication.Dtos.ProductoDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Result.ApiResponses;
using ProductApp.Aplication.Services;

namespace ProductApp.Api.Controllers.Modulo_Productos
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductoController : ControllerBase
    {
        private readonly IProductoServices _productoServices;

        public ProductoController(IProductoServices productoServices) 
        {
            _productoServices = productoServices;

        
        }

        [Authorize]
        [HttpPost("CreateProducto")]

        public async Task<IActionResult> CreateProducto(CreateProductoDto dto)
        {
            try
            {
                var result = await _productoServices.CreateAsync(dto);

                if (!result.IsSuccess)
                    return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

                return Ok(ApiResponseT<ProductoResponseDto>.SuccessResponse(result.Data, result.Message));


            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseT<Object>.FailureResponse(ex.Message));
            }

        }
        [Authorize]
        [HttpGet("GetAllProductos")]

        public async Task<IActionResult> GetAllProductos()
        {
            try
            {
                var result = await _productoServices.GetAllAsync();


                if (!result.IsSuccess)
                    return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));


                return Ok(ApiResponseT<List<ProductoResponseDto>>.SuccessResponse(result.Data, result.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseT<Object>.FailureResponse(ex.Message));

            }

        }


        [Authorize]
        [HttpGet("GetProductoById/{id}")]

        public async Task<IActionResult> GetProductoById(int id)
        {
            try
            {
                var result = await _productoServices.GetByIdAsync(id);

                if (!result.IsSuccess)
                    return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

                return Ok(ApiResponseT<ProductoResponseDto>.SuccessResponse(result.Data, result.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseT<Object>.FailureResponse(ex.Message));
            }

        }


        [Authorize]
        [HttpPut("UpdateProducto/{id}")]

        public async Task<IActionResult> UpdateProducto( int id , UpdateProductoDto dto)
        {
            try
            {
               dto.Id = id;
                var result = await _productoServices.UpdateAsync(dto);
                if (!result.IsSuccess)
                    return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

                return Ok(ApiResponseT<ProductoResponseDto>.SuccessResponse(result.Data, result.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseT<Object>.FailureResponse(ex.Message));
            }
        }

        [Authorize]
        [HttpPatch("DisableProducto/{id}")]

        public async Task<IActionResult> DisableProducto(int id)
        {
            try
            {
                var result = await _productoServices.DisableAsync(id);
                if (!result.IsSuccess)
                    return BadRequest(ApiResponse.FailureResponse(result.Message));
                return Ok(ApiResponse.SuccessResponse(result.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.FailureResponse(ex.Message));
            }
        }
    }




}

