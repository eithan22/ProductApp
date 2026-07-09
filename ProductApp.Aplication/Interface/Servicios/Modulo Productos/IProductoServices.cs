using ProductApp.Aplication.Common;
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

        Task<OperationResultD<PagedResult<ProductoResponseDto>>> GetAllAsync(bool incluirInactivos, int pageNumber = 1, int pageSize = 10);

        Task<OperationResultD<List<ProductoResponseDto>>> BuscarProductosPorNombreOCategoria(string? nombre, string? categoria, bool incluirInactivos = false);
      

        



    }
}
