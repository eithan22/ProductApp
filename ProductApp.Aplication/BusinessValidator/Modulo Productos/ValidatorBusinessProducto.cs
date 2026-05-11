using ProductApp.Aplication.Dtos.ProductoDto;
using ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Producto;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.BusinessValidator.Modulo_Productos
{
    public class ValidatorBusinessProducto : IValidatorBusinessProducto
    {
        private readonly IProductoRepository _productoRepository;

        public ValidatorBusinessProducto(IProductoRepository productoRepository)
        {
            _productoRepository = productoRepository;
        }
       

        public async Task<OperationResult> ValidarCreateProductoAsync(CreateProductoDto dto)
        {
           if(await _productoRepository.ExisteAsync(p => p.Nombre == dto.Nombre))
            {
                return OperationResult.Failure("Este nombre ya existe");
            }

            if (await _productoRepository.ExisteAsync(p => p.Descripcion == dto.Descripcion))
            {
                return OperationResult.Failure("Este descripcion ya existe");
            }

            return OperationResult.Success();    
        }






        public async Task<OperationResult> ValidarDisableProductoAsync(Producto producto)
        {
          if( producto.IsDisable == true)
            {
                return OperationResult.Failure("El producto ya esta inactivo");
               
            }

            return OperationResult.Success();
        }




        public async Task<OperationResult> ValidarUpdateProductoAsync(UpdateProductoDto dto, Producto producto)
        {
            if (await _productoRepository.ExisteAsync(p => p.Nombre == dto.Nombre))
            {
                return OperationResult.Failure("Este nombre ya existe");
            }

            if (await _productoRepository.ExisteAsync(p => p.Descripcion == dto.Descripcion))
            {
                return OperationResult.Failure("Este nombre ya existe");
            }

           


            return OperationResult.Success();

        }
    }
}
