using ProductApp.Aplication.Dtos.CategoriaDto;
using ProductApp.Aplication.Dtos.ProductoDto;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Entitis;
using System;
using System.Collections.Generic;
using System.Text;
using OperationResult = ProductApp.Aplication.Result.OperationResult.OperationResult;

namespace ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Producto
{
    public interface IValidatorBusinessProducto
    {
        Task<OperationResult> ValidarCreateProductoAsync(CreateProductoDto dto);

        Task<OperationResult> ValidarUpdateProductoAsync(UpdateProductoDto dto, Producto producto);

        Task<OperationResult> ValidarDisableProductoAsync(Producto producto);
    }
}
