using FluentValidation;
using Microsoft.Extensions.Logging;
using ProductApp.Aplication.Common;
using ProductApp.Aplication.Dtos.CategoriaDto;
using ProductApp.Aplication.Dtos.ProductoDto;
using ProductApp.Aplication.Dtos.UsuarioDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Interface.IMappers.Modulos_Productos;
using ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Producto;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;

namespace ProductApp.Aplication.Services
{
    public class ProductoServices : IProductoServices
    {
        private const int CantidadMinimaDefectoRespaldo = 5;

        private readonly IProductoRepository _productorepository;
        private readonly IInventarioRepository _inventarioRepository;
        private readonly IConfiguracionSistemaRepository _configuracionSistemaRepository;
        private readonly IMapperProducto _mapperProductoMapper;
        private readonly IValidator<CreateProductoDto> _validatorCreateProductoDto;
        private readonly IValidator<UpdateProductoDto> _validatorUpdateProductoDto;
        private readonly IValidator<SubirImagenProductoDto> _validatorSubirImagenProductoDto;
        private readonly IValidatorBusinessProducto _validatorBusinessProducto;
        private readonly IAlmacenamientoImagenes _almacenamientoImagenes;
        private readonly ILogger<ProductoServices> _logger;

        public ProductoServices
            (IProductoRepository productorepository,
            IMapperProducto mapperProductoMapper,
            IValidator<CreateProductoDto> validatorCreateProductoDto,
            IValidator<UpdateProductoDto> validatorUpdateProductoDto,
            IValidator<SubirImagenProductoDto> validatorSubirImagenProductoDto,
            IValidatorBusinessProducto validatorBusinessProducto,
            IInventarioRepository inventarioRepository,
            IConfiguracionSistemaRepository configuracionSistemaRepository,
            IAlmacenamientoImagenes almacenamientoImagenes,
            ILogger<ProductoServices> logger
            )
        {
            _productorepository = productorepository;
            _mapperProductoMapper = mapperProductoMapper;
            _validatorCreateProductoDto = validatorCreateProductoDto;
            _validatorUpdateProductoDto = validatorUpdateProductoDto;
            _validatorBusinessProducto = validatorBusinessProducto;
            _inventarioRepository = inventarioRepository;
            _configuracionSistemaRepository = configuracionSistemaRepository;
            _validatorSubirImagenProductoDto = validatorSubirImagenProductoDto;
            _almacenamientoImagenes = almacenamientoImagenes;
            _logger = logger;

        }
        /*

        Registrar nuevos productos.
       • Editar información de productos.
        • Asignar productos a una categoría.
        • Actualizar precio de venta.
         • Activar o desactivar productos.

        • Consultar lista de productos disponibles.
         • Buscar productos por nombre o categoría

        */



        //no se usara por ahora
        public async Task<OperationResultD<bool>> DeleteAsync(int id)
        {
            if (id <= 0)
            {
               return OperationResultD<bool>.Failure("El id no puede ser menor que 0");
            }

            var result = await _productorepository.GetByIdAsync(id);

            if(result == null)
            {
                return OperationResultD<bool>.Failure("El producto no fue encontrado");
            }

            await _productorepository.DeleteAsync(id);
            return OperationResultD<bool>.Success(true, "Producto eliminado exitosamente");



        }

        
        //desactivar un producto

        public async Task<OperationResultD<bool>> DisableAsync(int id)
        {
            if (id <= 0)
            {
                return OperationResultD<bool>.Failure("El id no puede ser menor a 0");
            }
            var producto = await _productorepository.GetByIdAsync(id);

            if (producto == null)
            {
                return OperationResultD<bool>.Failure("El producto no fue encontrado");

            }

            var validatoBusiness = await _validatorBusinessProducto.ValidarDisableProductoAsync(producto);

            if (!validatoBusiness.IsSuccess)
            {
                return OperationResultD<bool>.Failure(validatoBusiness.Message);
            }

            producto.DesactivarProducto();
            await _productorepository.UpdateAsync(producto);

            return OperationResultD<bool>.Success(true, "Producto deshabilitado exitosamente");

        }







        //crear un producto y agregarle una categoria ya creada

         public async Task<OperationResultD<ProductoResponseDto>> CreateAsync(CreateProductoDto dto)
        {

            var dtoValidator = await _validatorCreateProductoDto.ValidateAsync(dto);

            if (!dtoValidator.IsValid)
            {
                var errors = string.Join("; ", dtoValidator.Errors.Select(e => e.ErrorMessage));
                return OperationResultD<ProductoResponseDto>.Failure($"Error de validación: {errors}");
            }


            var validatorBusiness = await _validatorBusinessProducto.ValidarCreateProductoAsync(dto);

            if (!validatorBusiness.IsSuccess)
            {
                return OperationResultD<ProductoResponseDto>.Failure(validatorBusiness.Message);
            }

            var producto = _mapperProductoMapper.MapToCreateProducto(dto);

            

            await _productorepository.CreateAsync(producto);


            //crear un inventario para el producto creado con cantidad actual 0 y la cantidad minima configurada por defecto

            var configuracion = await _configuracionSistemaRepository.ObtenerAsync();

            var inventario = new Inventario(
             0,
             configuracion?.CantidadMinimaInventarioDefecto ?? CantidadMinimaDefectoRespaldo,
             producto.Id
             );

            await _inventarioRepository.CreateAsync(inventario);

            // Mismo criterio que el update: se recarga con Categoria, Inventario y Proveedor para
            // que la respuesta del POST traiga el stock recién creado y el nombre de la categoría,
            // en vez de los null que deja la entidad construida a mano por el mapper.
            var productoCreado = await _productorepository.GetProductoConCategoriaByIdAsync(producto.Id) ?? producto;

            var productoresponsedto = _mapperProductoMapper.MapToProductoResponse(productoCreado);

            return OperationResultD<ProductoResponseDto>.Success(productoresponsedto, "Producto creado correctamente");

        }





        //ver todos os productos
       public Task<OperationResultD<PagedResult<ProductoResponseDto>>> GetAllAsync(int pageNumber = 1, int pageSize = PaginacionDefaults.PageSizeDefault)
            => GetAllAsync(incluirInactivos: false, pageNumber, pageSize);

       public async Task<OperationResultD<PagedResult<ProductoResponseDto>>> GetAllAsync(bool incluirInactivos, int pageNumber = 1, int pageSize = PaginacionDefaults.PageSizeDefault)
        {
            if (pageNumber < 1)
                return OperationResultD<PagedResult<ProductoResponseDto>>.Failure("pageNumber debe ser mayor o igual a 1");

            if (pageSize < 1 || pageSize > 100)
                return OperationResultD<PagedResult<ProductoResponseDto>>.Failure("pageSize debe estar entre 1 y 100");

            var (productos, totalCount) = await _productorepository.GetAllConCategoriaAsync(incluirInactivos, pageNumber, pageSize);

            var productoresponsedto = productos.Select(c => _mapperProductoMapper.MapToProductoResponse(c)).ToList();

            var pagedResult = new PagedResult<ProductoResponseDto>
            {
                Items = productoresponsedto,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return OperationResultD<PagedResult<ProductoResponseDto>>.Success(pagedResult, "Productos obtenidos correctamente");
        }

        
        //ver un producto en especifico

        public async Task<OperationResultD<ProductoResponseDto>> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                return OperationResultD<ProductoResponseDto>.Failure("el id es invalido ");
            }

           var producto = await _productorepository.GetProductoConCategoriaByIdAsync(id);

            if (producto == null)
            {
               return OperationResultD<ProductoResponseDto>.Failure("Producto no encontrado");

            }
           
            var productoresponsedto = _mapperProductoMapper.MapToProductoResponse(producto);

            
            return OperationResultD<ProductoResponseDto>.Success(productoresponsedto, "Producto obtenido correctamente");



           
        }

        //actualizar el producto 
         public async Task<OperationResultD<ProductoResponseDto>> UpdateAsync(UpdateProductoDto dto)
        {
            var dtoValidator = await _validatorUpdateProductoDto.ValidateAsync(dto);

            if (!dtoValidator.IsValid)
            {
                var errors = string.Join("; ", dtoValidator.Errors.Select(e => e.ErrorMessage));
                return OperationResultD<ProductoResponseDto>.Failure($"Error de validación: {errors}");
            }


            var producto = await _productorepository.GetByIdAsync(dto.Id);

            if (producto == null)
            {
                return OperationResultD<ProductoResponseDto>.Failure("producto no encontrado");
            }

            var validatorBusiness = await _validatorBusinessProducto.ValidarUpdateProductoAsync(dto , producto);

            if(!validatorBusiness.IsSuccess)
            {
                return OperationResultD<ProductoResponseDto>.Failure(validatorBusiness.Message);
            }

            _mapperProductoMapper.MapToUpdateProducto(dto, producto);

            await _productorepository.UpdateAsync(producto);

            // Se recarga con Categoria, Inventario y Proveedor para que la respuesta del PUT
            // traiga los mismos campos que el listado (nombre de categoría y stock). Recargar
            // DESPUÉS de guardar y no antes es lo que hace que el nombre de la categoría sea
            // el nuevo cuando el update cambió de categoría.
            var productoActualizado = await _productorepository.GetProductoConCategoriaByIdAsync(dto.Id) ?? producto;

            var productoresponsedto = _mapperProductoMapper.MapToProductoResponse(productoActualizado);

            return OperationResultD<ProductoResponseDto>.Success(productoresponsedto, "Producto actualizado correctamente");
        }



        //reactivar producto
        public async Task<OperationResultD<bool>> EnableProducto(int id)
        {
            if (id <= 0)
            {
                return OperationResultD<bool>.Failure("El id no ppuede ser negativo o 0");
            }

            var producto = await _productorepository.GetByIdAsync(id);

            if (producto == null) 
            {
                return OperationResultD<bool>.Failure("El producto no se encontro");
            }

            producto.ActivarProducto();

            await _productorepository.UpdateAsync(producto);

            return OperationResultD<bool>.Success(true, "Producto activado");
        }




        //buscar productor por nombre y categoria
        public async Task<OperationResultD<List<ProductoResponseDto>>> BuscarProductosPorNombreOCategoria(string? nombre, string? categoria, bool incluirInactivos = false)
        {
            if (string.IsNullOrEmpty(nombre) && string.IsNullOrEmpty(categoria))
            {
                return OperationResultD<List<ProductoResponseDto>>.Failure("Debe proporcionar al menos un criterio de búsqueda");

            }



            var producto = await _productorepository.BuscarProductosAsync(nombre, categoria, incluirInactivos);

            if (producto.Count == 0)
            {
                return OperationResultD<List<ProductoResponseDto>>
                    .Success(new List<ProductoResponseDto>(), "No se encontraron productos con ese criterio");
            }

            var ProductoResponse = producto.Select(p => _mapperProductoMapper.MapToProductoResponse(p)).ToList();

            return OperationResultD<List<ProductoResponseDto>>.Success(ProductoResponse, "Productos encontrados Correctamente");
        }




        //subir o reemplazar la imagen de un producto ya existente
        public async Task<OperationResultD<ProductoResponseDto>> SubirImagenAsync(SubirImagenProductoDto dto)
        {
            var dtoValidator = await _validatorSubirImagenProductoDto.ValidateAsync(dto);

            if (!dtoValidator.IsValid)
            {
                var errors = string.Join("; ", dtoValidator.Errors.Select(e => e.ErrorMessage));
                return OperationResultD<ProductoResponseDto>.Failure($"Error de validación: {errors}");
            }

            // La extensión y el Content-Type del request no prueban nada: cualquiera puede
            // llamar "foto.jpg" a un ejecutable. Lo único confiable es la firma del contenido.
            var formato = await DetectorFormatoImagen.DetectarAsync(dto.Contenido);

            if (formato is null)
            {
                _logger.LogWarning(
                    "Se rechazó la imagen {NombreArchivo} del producto {ProductoId}: el contenido no corresponde a JPEG, PNG ni WEBP",
                    dto.NombreArchivo, dto.ProductoId);

                return OperationResultD<ProductoResponseDto>.Failure(
                    "El contenido del archivo no es una imagen válida. Se aceptan JPEG, PNG y WEBP.");
            }

            var producto = await _productorepository.GetProductoConCategoriaByIdAsync(dto.ProductoId);

            if (producto == null)
            {
                return OperationResultD<ProductoResponseDto>.Failure("Producto no encontrado");
            }

            var imagenAnterior = producto.ImagenUrl;

            // Se sube con la extensión y el Content-Type que salieron de la firma real, no con los
            // declarados: el contenedor es de lectura pública y el blob debe servirse como imagen.
            var nombreNormalizado = Path.ChangeExtension(dto.NombreArchivo, formato.Extension);

            var nuevaUrl = await _almacenamientoImagenes.SubirAsync(dto.Contenido, nombreNormalizado, formato.ContentType);

            producto.AsignarImagen(nuevaUrl);

            await _productorepository.UpdateAsync(producto);

            // El blob viejo se borra recién después de persistir la nueva url: si la subida o el
            // guardado fallan, el producto conserva una imagen que sigue existiendo en el storage.
            if (!string.IsNullOrWhiteSpace(imagenAnterior) && imagenAnterior != nuevaUrl)
            {
                try
                {
                    await _almacenamientoImagenes.EliminarAsync(imagenAnterior);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "No se pudo eliminar la imagen anterior {ImagenAnterior} del producto {ProductoId}",
                        imagenAnterior, producto.Id);
                }
            }

            var productoresponsedto = _mapperProductoMapper.MapToProductoResponse(producto);

            return OperationResultD<ProductoResponseDto>.Success(productoresponsedto, "Imagen del producto actualizada correctamente");
        }
    }
}
