using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Aplication.Common;
using ProductApp.Aplication.Dtos.CategoriaDto;
using ProductApp.Aplication.Dtos.Modulo_Productos.InventarioDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Result.ApiResponses;
using System.Security.Claims;

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


        [Authorize]
        [HttpGet("GetInventarioPorProducto/{productoId}")]
        public async Task<IActionResult> GetInventario(int productoId)
        {
            var result = await _inventarioService.ObtenerInventarioAsync(productoId);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<InventarioResponseDto>.SuccessResponse(result.Data, result.Message));
        }




        [Authorize]
        [HttpGet("GetAllInventarios")]
        public async Task<IActionResult> GetAllInventarios([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = PaginacionDefaults.PageSizeDefault, [FromQuery] int? proveedorId = null)
        {
            var result = await _inventarioService.ObtenerTodosInventariosAsync(pageNumber, pageSize, proveedorId);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<PagedResult<InventarioResponseDto>>.SuccessResponse(result.Data, result.Message));
        }


        [Authorize]
        [HttpGet("GetStockBajo")]
        public async Task<IActionResult> GetStockBajo([FromQuery] int? proveedorId = null)
        {
            var result = await _inventarioService.ObtenerStockBajoAsync(proveedorId);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<List<InventarioResponseDto>>.SuccessResponse(result.Data, result.Message));
        }


        [Authorize(Roles = "Administrador")]
        [HttpPost("AgregarStock")]

        public async Task<IActionResult> AgregarStockAsync( MovimientoStockDto dto)
        {
            var usuarioSolicitanteId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _inventarioService.AgregarStockAsync(dto, usuarioSolicitanteId);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<InventarioResponseDto>.SuccessResponse(result.Data, result.Message));
        }



        [Authorize(Roles = "Administrador")]
        [HttpPost("DescontarStock/{productoId}")]

        public async Task<IActionResult> DescontarStockAsync(int productoId, MovimientoStockDto dto)
        {
            dto.ProductoId = productoId;

            var usuarioSolicitanteId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _inventarioService.DescontarStockAsync(dto, usuarioSolicitanteId);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<InventarioResponseDto>.SuccessResponse(result.Data, result.Message));
        }




        [Authorize(Roles = "Administrador")]
        [HttpPut("AjustarInventario")]
        public async Task<IActionResult> AjustarInventario([FromQuery] int productoId, AjustarStockDto dto)
        {
            dto.ProductoId = productoId;

            var usuarioSolicitanteId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _inventarioService.AjustarStockAsync(dto, usuarioSolicitanteId);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<InventarioResponseDto>.SuccessResponse(result.Data, result.Message));
        }





    }

    }