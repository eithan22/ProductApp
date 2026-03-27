using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Aplication.Dtos.ClienteDto;
using ProductApp.Aplication.Interface;
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


        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Bienvenido");

        }



        [HttpPost(" Create Clientes")]

        public async Task<IActionResult> CreateCliente([FromBody] CreateClienteDto dto)
        {
            try
            {
                var result = await _clienteService.CreateAsync(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }

        }

        // get 
        [HttpGet(" Get Clientes")]
        public async Task<IActionResult> GetClientes()
        {
            var clientes = await _clienteService.GetAllAsync();
            return Ok(clientes);
        }

        [HttpGet("{id}")]

        public async Task<IActionResult> GetByIdClientes(int id)
        {
            try
            {
               var clientes =  await _clienteService.GetByIdAsync(id);
                return Ok(clientes);

            }catch (Exception ex)
            {
                return BadRequest(ex.Message); 
            }

        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> DisableClientes(int id)
        {

            try
            {
                await _clienteService.DisableAsync(id);

                return Ok("Cliente deshabilitado");

            }
            catch (Exception ex) 
            { 
                return BadRequest(new {mensaje = ex.Message});
            
            }
            
        }

        [HttpPut("{id}")]

        public async Task<IActionResult> UpdateCliente(int id ,[FromBody] UpdateClienteDto dto)
        {
            try
            {
                dto.Id = id;

                await _clienteService.UpdateAsync( dto);
                return Ok("Cliente Actualizado");

            }
            catch (Exception ex) 
            { 
                return BadRequest(new {mesaje = ex.Message}); 
            }

        }






    }
}
