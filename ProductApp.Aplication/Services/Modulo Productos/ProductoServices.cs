using FluentValidation;
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
        private const int CantidadMinimaDefecto = 5;

        private readonly IProductoRepository _productorepository;
        private readonly IInventarioRepository _inventarioRepository;
        private readonly IMapperProducto _mapperProductoMapper;
        private readonly IValidator<CreateProductoDto> _validatorCreateProductoDto;
        private readonly IValidator<UpdateProductoDto> _validatorUpdateProductoDto;
        private readonly IValidatorBusinessProducto _validatorBusinessProducto;

        public ProductoServices
            (IProductoRepository productorepository,
            IMapperProducto mapperProductoMapper,
            IValidator<CreateProductoDto> validatorCreateProductoDto,
            IValidator<UpdateProductoDto> validatorUpdateProductoDto,
            IValidatorBusinessProducto validatorBusinessProducto,
            IInventarioRepository inventarioRepository
            )
        {
            _productorepository = productorepository;
            _mapperProductoMapper = mapperProductoMapper;
            _validatorCreateProductoDto = validatorCreateProductoDto;
            _validatorUpdateProductoDto = validatorUpdateProductoDto;
            _validatorBusinessProducto = validatorBusinessProducto;
            _inventarioRepository = inventarioRepository;

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


            //crear un inventario para el producto creado con cantidad actual 0 y cantidad minima 5 por defecto

            var inventario = new Inventario(
             0,
             CantidadMinimaDefecto,
             producto.Id
             );

            await _inventarioRepository.CreateAsync(inventario);

            var productoresponsedto = _mapperProductoMapper.MapToProductoResponse(producto);

            return OperationResultD<ProductoResponseDto>.Success(productoresponsedto, "Producto creado correctamente");

        }





        //ver todos os productos 
       public async Task<OperationResultD<List<ProductoResponseDto>>> GetAllAsync()
        {
            var productos = await _productorepository.GetAllConCategoriaAsync();

            if(productos == null)
            {
                return OperationResultD<List<ProductoResponseDto>>.Failure("No se encontraron los productos");
            };


            var productoresponsedto = productos.Select(c => _mapperProductoMapper.MapToProductoResponse(c)).ToList();
               
               return OperationResultD<List<ProductoResponseDto>>.Success(productoresponsedto, "Productos obtenidos correctamente");
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

            var productoresponsedto = _mapperProductoMapper.MapToProductoResponse(producto);

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
        public async Task<OperationResultD<List<ProductoResponseDto>>> BuscarProductosPorNombreOCategoria(string? nombre, string? categoria)
        {
            if (string.IsNullOrEmpty(nombre) && string.IsNullOrEmpty(categoria)) 
            {
                return OperationResultD<List<ProductoResponseDto>>.Failure("Debe proporcionar al menos un criterio de búsqueda");
            
            }

           

            var producto = await _productorepository.BuscarProductosAsync(nombre, categoria);

            //mas adelante podre el si existe 
            if (producto.Count == 0)
            {
                return OperationResultD<List<ProductoResponseDto>>
                    .Failure("No se encontraron productos");
            }

            var ProductoResponse = producto.Select(p => _mapperProductoMapper.MapToProductoResponse(p)).ToList();

            return OperationResultD<List<ProductoResponseDto>>.Success(ProductoResponse, "Productos encontrados Correctamente");
        }
    }
}
