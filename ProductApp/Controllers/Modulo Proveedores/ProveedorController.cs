using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Aplication.Common;
using ProductApp.Aplication.Dtos.Modulo_Proveedores.ProveedorDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Result.ApiResponses;

namespace ProductApp.Api.Controllers.Modulo_Proveedores
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProveedorController : ControllerBase
    {
        private readonly IProveedorServices _proveedorService;

        public ProveedorController(IProveedorServices proveedorService)
        {
            _proveedorService = proveedorService;
        }

        [Authorize]
        [HttpPost("CreateProveedor")]
        public async Task<IActionResult> CreateProveedor(CreateProveedorDto dto)
        {
            var result = await _proveedorService.CreateAsync(dto);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<ProveedorResponseDto>.SuccessResponse(result.Data, result.Message));
        }

        [Authorize]
        [HttpGet("GetProveedores")]
        public async Task<IActionResult> GetProveedores([FromQuery] bool incluirInactivos = false, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = PaginacionDefaults.PageSizeDefault)
        {
            var result = await _proveedorService.GetAllAsync(incluirInactivos, pageNumber, pageSize);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<PagedResult<ProveedorResponseDto>>.SuccessResponse(result.Data, result.Message));
        }

        [Authorize]
        [HttpGet("GetByIdProveedor/{id}")]
        public async Task<IActionResult> GetByIdProveedor(int id)
        {
            var result = await _proveedorService.GetByIdAsync(id);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<ProveedorResponseDto>.SuccessResponse(result.Data, result.Message));
        }

        [Authorize]
        [HttpPut("UpdateProveedor/{id}")]
        public async Task<IActionResult> UpdateProveedor(int id, UpdateProveedorDto dto)
        {
            dto.Id = id;

            var result = await _proveedorService.UpdateAsync(dto);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<ProveedorResponseDto>.SuccessResponse(result.Data, result.Message));
        }

        [Authorize]
        [HttpPatch("DisableProveedor/{id}")]
        public async Task<IActionResult> DisableProveedor(int id)
        {
            var result = await _proveedorService.DisableAsync(id);

            if (!result.IsSuccess)
                return BadRequest(ApiResponse.FailureResponse(result.Message));

            return Ok(ApiResponse.SuccessResponse(result.Message));
        }

        [Authorize]
        [HttpPatch("EnableProveedor/{id}")]
        public async Task<IActionResult> EnableProveedor(int id)
        {
            var result = await _proveedorService.EnableProveedor(id);

            if (!result.IsSuccess)
                return BadRequest(ApiResponse.FailureResponse(result.Message));

            return Ok(ApiResponse.SuccessResponse(result.Message));
        }

        [Authorize]
        [HttpGet("GetBuscar")]
        public async Task<IActionResult> BuscarAsync(string? nombre, [FromQuery] bool incluirInactivos = false)
        {
            var result = await _proveedorService.BuscarAsync(nombre, incluirInactivos);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<List<ProveedorResponseDto>>.SuccessResponse(result.Data, result.Message));
        }
    }
}
