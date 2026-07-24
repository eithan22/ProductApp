using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Aplication.Common;
using ProductApp.Aplication.Dtos.ProductoDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Result.ApiResponses;

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
            var result = await _productoServices.CreateAsync(dto);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<ProductoResponseDto>.SuccessResponse(result.Data, result.Message));
        }
        [Authorize]
        [HttpGet("GetAllProductos")]

        public async Task<IActionResult> GetAllProductos([FromQuery] bool incluirInactivos = false, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _productoServices.GetAllAsync(incluirInactivos, pageNumber, pageSize);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<PagedResult<ProductoResponseDto>>.SuccessResponse(result.Data, result.Message));
        }


        [Authorize]
        [HttpGet("GetProductoById/{id}")]

        public async Task<IActionResult> GetProductoById(int id)
        {
            var result = await _productoServices.GetByIdAsync(id);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<ProductoResponseDto>.SuccessResponse(result.Data, result.Message));
        }


        [Authorize]
        [HttpPut("UpdateProducto/{id}")]

        public async Task<IActionResult> UpdateProducto( int id , UpdateProductoDto dto)
        {
            dto.Id = id;
            var result = await _productoServices.UpdateAsync(dto);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<ProductoResponseDto>.SuccessResponse(result.Data, result.Message));
        }

        [Authorize(Roles = "Administrador")]
        [HttpPatch("DisableProducto/{id}")]

        public async Task<IActionResult> DisableProducto(int id)
        {
            var result = await _productoServices.DisableAsync(id);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse.FailureResponse(result.Message));
            return Ok(ApiResponse.SuccessResponse(result.Message));
        }


        [Authorize(Roles = "Administrador")]
        [HttpPatch("EnableProducto/{id}")]

        public async Task<IActionResult> EnableProducto(int id)
        {
            var result = await _productoServices.EnableProducto(id);

            if (!result.IsSuccess)
                return BadRequest(ApiResponse.FailureResponse(result.Message));

            return Ok(ApiResponse.SuccessResponse(result.Message));
        }


        [Authorize]
        [HttpGet("BuscarProductos")]

        public async Task<IActionResult> BuscarProducto(string? nombre, string? categoria, [FromQuery] bool incluirInactivos = false)
        {
            var result = await _productoServices.BuscarProductosPorNombreOCategoria(nombre, categoria, incluirInactivos);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<List<ProductoResponseDto>>.SuccessResponse(result.Data, result.Message));
        }



    }




}

