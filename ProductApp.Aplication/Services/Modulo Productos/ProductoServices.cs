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
using ProductApp.Infraesctructura.Persistencia.Configuraciones;
using ProductApp.Infraesctructura.Persistencia.Repository;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace ProductApp.Aplication.Services
{
    public class ProductoServices : IProductoServices
    {
        private readonly IProductoRepository _productorepository;
        private readonly IMapperProducto _mapperProductoMapper;
        private readonly IValidator<CreateProductoDto> _validatorCreateProductoDto;
        private readonly IValidator<UpdateProductoDto> _validatorUpdateProductoDto;
        private readonly IValidatorBusinessProducto _validatorBusinessProducto;

        public ProductoServices
            (IProductoRepository productorepository,
            IMapperProducto mapperProductoMapper,
            IValidator<CreateProductoDto> validatorCreateProductoDto,
            IValidator<UpdateProductoDto> validatorUpdateProductoDto,
            IValidatorBusinessProducto validatorBusinessProducto)
        {
            _productorepository = productorepository;
            _mapperProductoMapper = mapperProductoMapper;
            _validatorCreateProductoDto = validatorCreateProductoDto;
            _validatorUpdateProductoDto = validatorUpdateProductoDto;
            _validatorBusinessProducto = validatorBusinessProducto;

        }

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

            await _productorepository.DisebleAsync(id);
            return OperationResultD<bool>.Success(true, "Producto deshabilitado exitosamente");

        }



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

            var producto =  _mapperProductoMapper.MapToCreateProducto(dto);

            

            await _productorepository.CreateAsync(producto);

            
           var productoresponsedto = _mapperProductoMapper.MapToProductoResponse(producto);

            return OperationResultD<ProductoResponseDto>.Success(productoresponsedto, "Producto creado correctamente");

        }



       public async Task<OperationResultD<List<ProductoResponseDto>>> GetAllAsync()
        {
            var productos = await _productorepository.GetAllAsync();

            if(productos == null)
            {
                return OperationResultD<List<ProductoResponseDto>>.Failure("No se encontraron los productos");
            };


            var productoresponsedto = productos.Select(c => _mapperProductoMapper.MapToProductoResponse(c)).ToList();
               
               return OperationResultD<List<ProductoResponseDto>>.Success(productoresponsedto, "Productos obtenidos correctamente");
        }

        

        public async Task<OperationResultD<ProductoResponseDto>> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                return OperationResultD<ProductoResponseDto>.Failure("el id es invalido ");
            }

           var producto = await _productorepository.GetByIdAsync(id);

            if (producto == null)
            {
               return OperationResultD<ProductoResponseDto>.Failure("Producto no encontrado");

            }
           
            var productoresponsedto = _mapperProductoMapper.MapToProductoResponse(producto);

            
            return OperationResultD<ProductoResponseDto>.Success(productoresponsedto, "Producto obtenido correctamente");



           
        }

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

      
    }
}
