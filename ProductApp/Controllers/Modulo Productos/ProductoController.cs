using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductApp.Aplication.Common;
using ProductApp.Aplication.Dtos.Modulo_Productos.ImportacionDto;
using ProductApp.Aplication.Dtos.ProductoDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Result.ApiResponses;
using System.Security.Claims;

namespace ProductApp.Api.Controllers.Modulo_Productos
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductoController : ControllerBase
    {
        private readonly IProductoServices _productoServices;
        private readonly IImportacionProductosService _importacionProductosService;

        public ProductoController(
            IProductoServices productoServices,
            IImportacionProductosService importacionProductosService)
        {
            _productoServices = productoServices;
            _importacionProductosService = importacionProductosService;


        }

        [Authorize]
        [HttpPost("CreateProducto")]

        public async Task<IActionResult> CreateProducto(CreateProductoDto dto)
        {
            var result = await _productoServices.CreateAsync(dto);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<ProductoResponseDto>.SuccessResponse(result.Data, result.Message));
        }
        [Authorize]
        [HttpGet("GetAllProductos")]

        public async Task<IActionResult> GetAllProductos([FromQuery] bool incluirInactivos = false, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _productoServices.GetAllAsync(incluirInactivos, pageNumber, pageSize);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<PagedResult<ProductoResponseDto>>.SuccessResponse(result.Data, result.Message));
        }


        [Authorize]
        [HttpGet("GetProductoById/{id}")]

        public async Task<IActionResult> GetProductoById(int id)
        {
            var result = await _productoServices.GetByIdAsync(id);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<ProductoResponseDto>.SuccessResponse(result.Data, result.Message));
        }


        [Authorize]
        [HttpPut("UpdateProducto/{id}")]

        public async Task<IActionResult> UpdateProducto( int id , UpdateProductoDto dto)
        {
            dto.Id = id;
            var result = await _productoServices.UpdateAsync(dto);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<ProductoResponseDto>.SuccessResponse(result.Data, result.Message));
        }

        [Authorize(Roles = "Administrador")]
        [HttpPatch("DisableProducto/{id}")]

        public async Task<IActionResult> DisableProducto(int id)
        {
            var result = await _productoServices.DisableAsync(id);
            if (!result.IsSuccess)
                return BadRequest(ApiResponse.FailureResponse(result.Message));
            return Ok(ApiResponse.SuccessResponse(result.Message));
        }


        [Authorize(Roles = "Administrador")]
        [HttpPatch("EnableProducto/{id}")]

        public async Task<IActionResult> EnableProducto(int id)
        {
            var result = await _productoServices.EnableProducto(id);

            if (!result.IsSuccess)
                return BadRequest(ApiResponse.FailureResponse(result.Message));

            return Ok(ApiResponse.SuccessResponse(result.Message));
        }


        [Authorize]
        [HttpPost("SubirImagen/{id}")]
        [RequestSizeLimit(6 * 1024 * 1024)]

        public async Task<IActionResult> SubirImagen(int id, [FromForm] SubirImagenProductoRequest request)
        {
            var archivo = request?.Archivo;

            if (archivo is null || archivo.Length == 0)
                return BadRequest(ApiResponseT<Object>.FailureResponse("Debe adjuntar un archivo de imagen."));

            await using var contenido = archivo.OpenReadStream();

            var dto = new SubirImagenProductoDto
            {
                ProductoId = id,
                Contenido = contenido,
                NombreArchivo = archivo.FileName,
                ContentType = archivo.ContentType,
                TamanoBytes = archivo.Length
            };

            var result = await _productoServices.SubirImagenAsync(dto);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<ProductoResponseDto>.SuccessResponse(result.Data, result.Message));
        }


        [Authorize]
        [HttpGet("BuscarProductos")]

        public async Task<IActionResult> BuscarProducto(string? nombre, string? categoria, [FromQuery] bool incluirInactivos = false)
        {
            var result = await _productoServices.BuscarProductosPorNombreOCategoria(nombre, categoria, incluirInactivos);
            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<List<ProductoResponseDto>>.SuccessResponse(result.Data, result.Message));
        }


        // Importación masiva de productos (RF-3.10). Solo Administrador: la historia de usuario
        // del PRD 3.10 es explícita ("Como Administrador..."), y un alta de 500 productos de
        // golpe es una operación de catálogo, no de mostrador.

        [Authorize(Roles = "Administrador")]
        [HttpGet("DescargarPlantillaImportacion")]
        public IActionResult DescargarPlantillaImportacion()
        {
            var result = _importacionProductosService.ObtenerPlantilla();

            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return File(result.Data!, ContenidoXlsx, "plantilla-importacion-productos.xlsx");
        }


        [Authorize(Roles = "Administrador")]
        [HttpPost("ImportarMasivo")]
        [RequestSizeLimit(6 * 1024 * 1024)]
        public async Task<IActionResult> ImportarMasivo([FromForm] ImportarProductosRequest request)
        {
            var archivo = request?.Archivo;

            if (archivo is null || archivo.Length == 0)
                return BadRequest(ApiResponseT<Object>.FailureResponse("Debe adjuntar un archivo .xlsx o .csv."));

            await using var contenido = archivo.OpenReadStream();

            var dto = new ImportarProductosDto
            {
                Contenido = contenido,
                NombreArchivo = archivo.FileName,
                TamanoBytes = archivo.Length
            };

            var usuarioSolicitanteId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var result = await _importacionProductosService.ImportarAsync(dto, usuarioSolicitanteId);

            if (!result.IsSuccess)
                return BadRequest(ApiResponseT<Object>.FailureResponse(result.Message));

            return Ok(ApiResponseT<ImportacionProductosResultadoDto>.SuccessResponse(result.Data, result.Message));
        }

        private const string ContenidoXlsx =
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    }

    // Swashbuckle no puede documentar un IFormFile recibido como parámetro suelto de acción
    // (rompe /swagger con 500): hay que envolverlo en una clase para [FromForm].
    public class SubirImagenProductoRequest
    {
        public IFormFile Archivo { get; set; } = null!;
    }

    // Mismo motivo que SubirImagenProductoRequest.
    public class ImportarProductosRequest
    {
        public IFormFile Archivo { get; set; } = null!;
    }

}

