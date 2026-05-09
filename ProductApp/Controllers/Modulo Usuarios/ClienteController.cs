using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            try
            {
                var result = await _clienteService.CreateAsync(dto);


                if(!result.IsSuccess)
                    return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

                
                   
                return Ok(ApiResponseT<ClienteResponseDto>.SuccessResponse(result.Data, result.Message));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseT<Object>.FailureResponse(ex.Message));
            }

        }
        [Authorize]
        // get 
        [HttpGet("GetClientes")]
        public async Task<IActionResult> GetClientes()
        {
            try
            {
                var result = await _clienteService.GetAllAsync();
                if (!result.IsSuccess)
                    return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));


                return Ok(ApiResponseT<List<ClienteResponseDto>>.SuccessResponse(result.Data, result.Message));

            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponseT<Object>.FailureResponse(ex.Message));

            }
        }



        [Authorize]

        [HttpGet("GetByIdClientes{id}")]

        public async Task<IActionResult> GetByIdClientes(int id)
        {
            try
            {
               var resut =  await _clienteService.GetByIdAsync(id);

                if (!resut.IsSuccess)
                    return BadRequest(ApiResponseT<Object>.FailureResponse(resut.Message));


                return Ok(ApiResponseT<ClienteResponseDto>.SuccessResponse(resut.Data, resut.Message));

            }catch (Exception ex)
            {
                return BadRequest(ApiResponseT<Object>.FailureResponse(ex.Message)); 
            }

        }

        [Authorize]

        [HttpDelete("DisableCliente{id}")]
        public async Task<IActionResult> DisableClientes(int id)
        {

            try
            {
              var result=  await _clienteService.DisableAsync(id);

                if(!result.IsSuccess)
                    return BadRequest(ApiResponse.FailureResponse(result.Message));

                return Ok(ApiResponse.SuccessResponse(result.Message)); //no usamos data porque solo queremos indicar que se deshabilito correctamente

            }
            catch (Exception ex) 
            { 
                return BadRequest(ApiResponse.FailureResponse(ex.Message));
            
            }
            
        }


        [Authorize]

        [HttpPut("UpdateCliente{id}")]

        public async Task<IActionResult> UpdateCliente(int id ,UpdateClienteDto dto)
        {
            try
            {
                dto.Id = id;

               var result = await _clienteService.UpdateAsync(dto);
                if(!result.IsSuccess)
                    return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

                return Ok(ApiResponseT<ClienteResponseDto>.SuccessResponse(result.Data, result.Message));//no usamos data porque solo queremos indicar que se actualizo correctamente

            }
            catch (Exception ex) 
            { 
                return BadRequest(ApiResponseT<Object>.FailureResponse(ex.Message)); 
            }

        }

        [Authorize]

        [HttpGet("GetBuscar")]

        public async Task<IActionResult> BuscarAsync(string? nombre, string? telefono, string? correo)
        {
            try
            {
                var result = await _clienteService.BuscarAsync(nombre, telefono, correo);
                if(!result.IsSuccess)
                 return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

                return Ok(ApiResponseT<List<ClienteResponseDto>>.SuccessResponse(result.Data, result.Message));
            }
            catch (Exception ex)
            {
                    return BadRequest(ApiResponseT<Object>.FailureResponse(ex.Message));

                }
            }
        }






    }

