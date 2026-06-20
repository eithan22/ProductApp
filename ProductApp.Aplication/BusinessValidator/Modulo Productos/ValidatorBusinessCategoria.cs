using ProductApp.Aplication.Dtos.CategoriaDto;
using ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Producto;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.BusinessValidator.Modulo_Productos
{
    public class ValidatorBusinessCategoria : IValidatorBusinessCategoria
    {
        private readonly ICategoriaRepository _categoriaRepository;

        public ValidatorBusinessCategoria(ICategoriaRepository categoriaRepository)
        {
            _categoriaRepository = categoriaRepository;
        }

        public async Task<OperationResult> ValidarCreateCategoriaAsync(CreateCategoriaDto dto)
        {
            if(await _categoriaRepository.ExisteAsync(c => c.Nombre == dto.Nombre))
            {
                return OperationResult.Failure("Este Nombre ya existe.");
            }

            if(await _categoriaRepository.ExisteAsync(c => c.Descripcion == dto.Descripcion))
            {
                return OperationResult.Failure("Esta Descripcion ya existe.");
            }

          

            return OperationResult.Success();
        }

        public async Task<OperationResult> ValidarDeleteCategoriaAsync(Categoria categoria)
        {
            if (categoria == null)
            {
                return OperationResult.Failure("La categoria no existe.");
            }
            if (categoria.EstaEliminado == true)
            {
                return OperationResult.Failure("La categoria ya está inactiva.");
            }
            return OperationResult.Success();

        }

        public async Task<OperationResult> ValidarUpdateCategoriaAsync(UpdateCategoriaDto dto, Categoria categoria)
        {
            if (categoria == null)
            {
                return OperationResult.Failure("La categoria no existe.");
            }

            if (await _categoriaRepository.ExisteAsync(c => c.Nombre == dto.Nombre && c.Id != categoria.Id))
            {
                return OperationResult.Failure("Este Nombre ya existe.");
            }

            if (await _categoriaRepository.ExisteAsync(c => c.Descripcion == dto.Descripcion && c.Id != categoria.Id))
            {
                return OperationResult.Failure("Esta Descripcion ya existe.");
            }
            return OperationResult.Success();

        }
    }
}
