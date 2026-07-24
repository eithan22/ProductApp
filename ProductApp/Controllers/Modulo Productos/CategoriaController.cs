using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Aplication.Common;
using ProductApp.Aplication.Dtos.CategoriaDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Result.ApiResponses;

namespace ProductApp.Api.Controllers.Modulo_Productos
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriaController : ControllerBase
    {
        private readonly ICategoriaServices _categoriaService;

        public CategoriaController(ICategoriaServices categoriaService)
        {
            _categoriaService = categoriaService;
        }

        [Authorize]
        [HttpPost("CreateCategoria")]


        public async Task<IActionResult> CreateCategoria(CreateCategoriaDto dto)
        {
            var result = await _categoriaService.CreateAsync(dto);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<CategoriaResponseDto>.SuccessResponse(result.Data, result.Message));
        }
        [Authorize]
        [HttpGet("GetAllCategorias")]

        public async Task<IActionResult> GetAllCategorias([FromQuery] bool incluirInactivos = false, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _categoriaService.GetAllAsync(incluirInactivos, pageNumber, pageSize);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<PagedResult<CategoriaResponseDto>>.SuccessResponse(result.Data, result.Message));
        }


        [Authorize]
        [HttpGet("GetCategoriaById/{id}")]

        public async Task<IActionResult> GetCategoriaById(int id)
        {
            var result = await _categoriaService.GetByIdAsync(id);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<CategoriaResponseDto>.SuccessResponse(result.Data, result.Message));
        }
        [Authorize]
        [HttpPut("UpdateCategoria/{id}")]
        public async Task<IActionResult> UpdateCategoria(int id, UpdateCategoriaDto dto)
        {
            dto.Id = id;
            var result = await _categoriaService.UpdateAsync(dto);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<CategoriaResponseDto>.SuccessResponse(result.Data, result.Message));
        }





        [Authorize]
        [HttpPatch("DisableCategoria/{id}")]

        public async Task<IActionResult> DisableCategoria(int id)
        {
            var result = await _categoriaService.DisableAsync(id);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse.FailureResponse(result.Message));
            return Ok(ApiResponse.SuccessResponse(result.Message));
        }


        [Authorize]
        [HttpPatch("EnableCategoria/{id}")]

        public async Task<IActionResult> EnableCategoria(int id)
        {
            var result = await _categoriaService.EnableCategoria(id);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse.FailureResponse(result.Message));
            return Ok(ApiResponse.SuccessResponse(result.Message));
        }
    }

    }