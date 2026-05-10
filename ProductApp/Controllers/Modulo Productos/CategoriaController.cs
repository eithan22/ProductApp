using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            try
            {
                var result = await _categoriaService.CreateAsync(dto);

                if (!result.IsSuccess)
                    return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

                return Ok(ApiResponseT<CategoriaResponseDto>.SuccessResponse(result.Data, result.Message));


            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseT<Object>.FailureResponse(ex.Message));
            }

        }
        [Authorize]
        [HttpGet("GetAllCategorias")]

        public async Task<IActionResult> GetAllCategorias()
        {
            try
            {
                var result = await _categoriaService.GetAllAsync();


                if (!result.IsSuccess)
                    return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));


                return Ok(ApiResponseT<List<CategoriaResponseDto>>.SuccessResponse(result.Data, result.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseT<Object>.FailureResponse(ex.Message));

            }

        }


        [Authorize]
        [HttpGet("GetCategoriaById/{id}")]

        public async Task<IActionResult> GetCategoriaById(int id)
        {
            try
            {
                var result = await _categoriaService.GetByIdAsync(id);

                if (!result.IsSuccess)
                    return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

                return Ok(ApiResponseT<CategoriaResponseDto>.SuccessResponse(result.Data, result.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseT<Object>.FailureResponse(ex.Message));
            }

        }
        [Authorize]
        [HttpPut("UpdateCategoria")]

        public async Task<IActionResult> UpdateCategoria( int id ,UpdateCategoriaDto dto)
        {
            try
            {
                dto.Id = id;
                var result = await _categoriaService.UpdateAsync(dto);
                if (!result.IsSuccess)
                    return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

                return Ok(ApiResponseT<CategoriaResponseDto>.SuccessResponse(result.Data, result.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseT<Object>.FailureResponse(ex.Message));
            }
        }

        [Authorize]
        [HttpPatch("DisableCategoria/{id}")]

        public async Task<IActionResult> DisableCategoria(int id)
        {
            try
            {
                var result = await _categoriaService.DisableAsync(id);
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