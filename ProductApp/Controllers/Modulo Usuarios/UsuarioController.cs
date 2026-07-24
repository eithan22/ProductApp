using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Api.Filters;
using ProductApp.Aplication.Common;
using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto;
using ProductApp.Aplication.Dtos.UsuarioDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Result.ApiResponses;
using ProductApp.Aplication.Result.OperationResult;
using System.Security.Claims;

namespace ProductApp.Api.Controllers.Modulo_Usuarios
{

    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuarioController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [Authorize(Roles = "Administrador")]
        [HttpGet("GetUsuarios")]
        public async Task<IActionResult> GetUsuarios([FromQuery] bool incluirInactivos = false, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _usuarioService.GetAllAsync(incluirInactivos, pageNumber, pageSize);
            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponseT<object>.FailureResponse(result.Message));
            }

            return Ok(ApiResponseT<PagedResult<UsuarioResponseDto>>.SuccessResponse(result.Data, result.Message));
        }


        [HttpGet("GetUsuarioById/{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> GetUsuarioById(int id)
        {
            var result = await _usuarioService.GetByIdAsync(id);
            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponseT<object>.FailureResponse(result.Message));
            }
            return Ok(ApiResponseT<UsuarioResponseDto>.SuccessResponse(result.Data, result.Message));
        }



        [HttpPost("CreateUsuario")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CreateUsuario( CreateUsuarioDto dto)
        {
            var result = await _usuarioService.CreateAsync(dto);
            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponseT<object>.FailureResponse(result.Message));
            }
            return Ok(ApiResponseT<UsuarioResponseDto>.SuccessResponse(result.Data, result.Message));
        }


        [HttpPut("UpdateUsuario/{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> UpdateUsuario(int id, UpdateUsuarioDto dto)
        {
            dto.Id = id;
            var result = await _usuarioService.UpdateAsync(dto);
            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponseT<object>.FailureResponse(result.Message));
            }
            return Ok(ApiResponseT<UsuarioResponseDto>.SuccessResponse(result.Data, result.Message));
        }


        /*

        [HttpDelete("DeleteUsuario/{id}")]

        [Authorize(Roles = "Administrador")]

        public async Task<IActionResult> DeleteUsuario(int id)
        {

            try
            {

                var result = await _usuarioService.DeleteAsync(id);
                if (!result.IsSuccess)
                {
                    return BadRequest(ApiResponse.FailureResponse(result.Message));
                }
                return Ok(ApiResponse.SuccessResponse(result.Message));

            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.FailureResponse(ex.Message));

            }

        }
        */




        [HttpPatch("DisableUsuario/{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Disable(int id)
        {
            var result = await _usuarioService.DisableAsync(id);
            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse.FailureResponse(result.Message));
            }
            return Ok(ApiResponse.SuccessResponse(result.Message));
        }


        [HttpPatch("EnableUsuario/{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EnableUsuario(int id)
        {
            var result = await _usuarioService.EnableUsuario(id);
            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse.FailureResponse(result.Message));
            }
            return Ok(ApiResponse.SuccessResponse(result.Message));
        }





        [HttpPost("CambiarPassword")]
        [Authorize]
        [PermitirConPasswordPendiente]
        public async Task<IActionResult> CambiarPassword(ChangePasswordDto dto)
        {
            // Obtener el ID del usuario desde el token JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Unauthorized("Token inválido");

            var userId = int.Parse(userIdClaim.Value);
            dto.Id = userId;

            var result = await _usuarioService.CambiarPasswordUsuario(dto);
            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse.FailureResponse(result.Message));
            }
            return Ok(ApiResponse.SuccessResponse(result.Message));
        }



        [HttpPut("CambiarRol")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CambiarRol(CambiarRolDto dto)
        {
            var result = await _usuarioService.CambiarRol(dto);
            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse.FailureResponse(result.Message));
            }
            return Ok(ApiResponse.SuccessResponse(result.Message));
        }


        [HttpPut("ResetearPassword")]
        [Authorize(Roles = "Administrador")]

        public async Task<IActionResult> ResetearPassword(ResetearPasswordDto dto)
        {
            var result = await _usuarioService.ResetearPassword(dto);
            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse.FailureResponse(result.Message));
            }
            return Ok(ApiResponse.SuccessResponse(result.Message));
        }

        [HttpGet("MiPerfil")]
        [Authorize]
        [PermitirConPasswordPendiente]
        public async Task<IActionResult> MiPerfil()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _usuarioService.ObtenerMiPerfilAsync(userId);
            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponseT<object>.FailureResponse(result.Message));
            }
            return Ok(ApiResponseT<UsuarioResponseDto>.SuccessResponse(result.Data, result.Message));
        }

        [HttpPut("MiPerfil")]
        [Authorize]
        public async Task<IActionResult> ActualizarMiPerfil(ActualizarMiPerfilDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _usuarioService.ActualizarMiPerfilAsync(userId, dto);
            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponseT<object>.FailureResponse(result.Message));
            }
            return Ok(ApiResponseT<UsuarioResponseDto>.SuccessResponse(result.Data, result.Message));
        }

    }

}

