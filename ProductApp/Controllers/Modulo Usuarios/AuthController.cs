using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto.AuthDto;
using ProductApp.Aplication.Dtos.UsuarioDto;
using ProductApp.Aplication.Interface.Servicios.Modulo_Usuarios;
using ProductApp.Aplication.Result.ApiResponses;
using ProductApp.Api.Filters;

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
        [EnableRateLimiting("login")] // aplica la política "login" (5 intentos por minuto) definida en Program.cs
        // El endpoint es anónimo, pero si el navegador todavía adjunta un token viejo de una
        // cuenta ya desactivada, VerificarSesionVigenteFilter lo cortaría con 401 antes de
        // llegar acá, impidiendo el nuevo login. El propio login no necesita esa verificación.
        [PermitirSinVerificarEstado]
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
