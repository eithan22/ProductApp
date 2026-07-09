using ProductApp.Aplication.Common;
using ProductApp.Aplication.Dtos.CategoriaDto;
using ProductApp.Aplication.Interface.Servicios.BaseServices;
using ProductApp.Aplication.Result.OperationResult;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Interface
{
    public interface ICategoriaServices : IBaseServices<CategoriaResponseDto, CreateCategoriaDto, UpdateCategoriaDto>
    {
        Task<OperationResultD<bool>> EnableCategoria(int id);

        Task<OperationResultD<PagedResult<CategoriaResponseDto>>> GetAllAsync(bool incluirInactivos, int pageNumber = 1, int pageSize = 10);
    }
}
