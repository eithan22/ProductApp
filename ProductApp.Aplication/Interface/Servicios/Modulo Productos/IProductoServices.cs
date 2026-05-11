using ProductApp.Aplication.Dtos.ProductoDto;
using ProductApp.Aplication.Dtos.UsuarioDto;
using ProductApp.Aplication.Interface.Servicios.BaseServices;
using ProductApp.Aplication.Result.OperationResult;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Interface
{
    public interface IProductoServices : IBaseServices<ProductoResponseDto, CreateProductoDto, UpdateProductoDto>
    {

         Task<OperationResultD<bool>>EnableProducto(int id);

        

        Task<OperationResultD<List<ProductoResponseDto>>> BuscarProductosPorNombreOCategoria(string? nombre, string? categoria);
      

        



    }
}
