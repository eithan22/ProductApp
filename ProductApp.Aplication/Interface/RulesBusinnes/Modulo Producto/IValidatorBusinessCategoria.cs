using ProductApp.Aplication.Dtos.CategoriaDto;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Entitis;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Producto
{
    public interface IValidatorBusinessCategoria
    {
        Task<OperationResult> ValidarCreateCategoriaAsync(CreateCategoriaDto dto);
        Task<OperationResult> ValidarUpdateCategoriaAsync(UpdateCategoriaDto dto, Categoria categoria);

        Task<OperationResult> ValidarDeleteCategoriaAsync(Categoria categoria);
    }
}
