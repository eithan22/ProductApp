using Microsoft.AspNetCore.Mvc;
using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto.AuthDto;
using ProductApp.Aplication.Dtos.UsuarioDto;
using ProductApp.Aplication.Interface.Servicios.Modulo_Usuarios;
using ProductApp.Aplication.Result.ApiResponses;

namespace ProductApp.Api.Controllers.Modulo_Usuarios
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]

        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _authService.Login(dto);
            if (!result.IsSuccess)
            {
                return Unauthorized(ApiResponseT<object>.FailureResponse(result.Message));
            }
            return Ok(ApiResponseT<AuthResponseDto>.SuccessResponse(result.Data, result.Message));
        }

    }
}
