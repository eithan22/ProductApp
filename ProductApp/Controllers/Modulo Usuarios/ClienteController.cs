using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Aplication.Common;
using ProductApp.Aplication.Dtos.ClienteDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Result.ApiResponses;
using ProductApp.Domian.Entitis;

namespace ProductApp.Api.Controllers.Modulo_Usuarios
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClienteController : ControllerBase
    {
        private readonly IClienteServices _clienteService;

        public ClienteController(IClienteServices clienteService)
        {
            _clienteService = clienteService;
        }





        [Authorize]
        [HttpPost("CreateClientes")]

        public async Task<IActionResult> CreateCliente(CreateClienteDto dto)
        {
            var result = await _clienteService.CreateAsync(dto);

            if(!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<ClienteResponseDto>.SuccessResponse(result.Data, result.Message));
        }
        [Authorize]
        // get 
        [HttpGet("GetClientes")]
        public async Task<IActionResult> GetClientes([FromQuery] bool incluirInactivos = false, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _clienteService.GetAllAsync(incluirInactivos, pageNumber, pageSize);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<PagedResult<ClienteResponseDto>>.SuccessResponse(result.Data, result.Message));
        }



        [Authorize]

        [HttpGet("GetByIdClientes/{id}")]

        public async Task<IActionResult> GetByIdClientes(int id)
        {
            var resut =  await _clienteService.GetByIdAsync(id);

            if (!resut.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(resut.Message));

            return Ok(ApiResponseT<ClienteResponseDto>.SuccessResponse(resut.Data, resut.Message));
        }

        [Authorize]

        [HttpPatch("DisableCliente/{id}")]
        public async Task<IActionResult> DisableClientes(int id)
        {
            var result=  await _clienteService.DisableAsync(id);

            if(!result.IsSuccess)
                return BadRequest(ApiResponse.FailureResponse(result.Message));

            return Ok(ApiResponse.SuccessResponse(result.Message)); //no usamos data porque solo queremos indicar que se deshabilito correctamente
        }


        [Authorize]

        [HttpPatch("EnableCliente/{id}")]
        public async Task<IActionResult> EnableCliente(int id)
        {
            var result = await _clienteService.EnableCliente(id);

            if (!result.IsSuccess)
                return BadRequest(ApiResponse.FailureResponse(result.Message));

            return Ok(ApiResponse.SuccessResponse(result.Message));
        }


        [Authorize]

        [HttpPut("UpdateCliente/{id}")]

        public async Task<IActionResult> UpdateCliente(int id ,UpdateClienteDto dto)
        {
            dto.Id = id;

            var result = await _clienteService.UpdateAsync(dto);
            if(!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<ClienteResponseDto>.SuccessResponse(result.Data, result.Message));//no usamos data porque solo queremos indicar que se actualizo correctamente
        }

        [Authorize]

        [HttpGet("GetBuscar")]

        //tambien puedo hacer un dto de busquedad pero asi sta bien 
        public async Task<IActionResult> BuscarAsync(string? nombre, string? telefono, string? correo, [FromQuery] bool incluirInactivos = false)
        {
            var result = await _clienteService.BuscarAsync(nombre, telefono, correo, incluirInactivos);
            if(!result.IsSuccess)
             return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<List<ClienteResponseDto>>.SuccessResponse(result.Data, result.Message));
        }
    }





    }

