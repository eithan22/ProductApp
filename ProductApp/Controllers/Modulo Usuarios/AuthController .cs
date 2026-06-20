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

        [HttpPost("register")]

        public async Task<IActionResult> Register(RegisteDto dto)
        {

            try
            {
                var result = await _authService.Register(dto);
                if (!result.IsSuccess)
                {
                    return BadRequest(ApiResponseT<object>.FailureResponse(result.Message));
                }

                return Ok(ApiResponseT<UsuarioResponseDto>.SuccessResponse(result.Data, result.Message));



            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseT<object>.FailureResponse(ex.Message));

            }
        }


        [HttpPost("login")]

        public async Task<IActionResult> Login(LoginDto dto)
        {
            try
            {
                var result = await _authService.Login(dto);
                if (!result.IsSuccess)
                {
                    return BadRequest(ApiResponseT<object>.FailureResponse(result.Message));
                }
                return Ok(ApiResponseT<AuthResponseDto>.SuccessResponse(result.Data, result.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseT<object>.FailureResponse(ex.Message));

            }



        }

    }
}
