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

            // La descripcion NO se valida como unica: dos categorias distintas pueden
            // compartir un texto descriptivo ("Productos varios") sin que eso sea un error.
            // El unico identificador del negocio es el Nombre, que ademas tiene indice
            // unico filtrado en base (UX_Categorias_Nombre).

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

            // Misma razon que en el create: la descripcion puede repetirse entre categorias.
            return OperationResult.Success();

        }
    }
}
