using FluentValidation;
using ProductApp.Aplication.Common;
using ProductApp.Aplication.Dtos.CategoriaDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Interface.IMappers.Modulos_Productos;
using ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Producto;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Entitis;
using ProductApp.Domian.Interfaces;

namespace ProductApp.Aplication.Services
{
    public class CategoriaServices : ICategoriaServices
    {
        private readonly ICategoriaRepository _categoriaRepository;
        private readonly IMapperCategoria _mapperCategoria;
        private readonly IValidator<CreateCategoriaDto> _createValidator;
        private readonly IValidator<UpdateCategoriaDto> _updateValidator;
        private readonly IValidatorBusinessCategoria _validatorBusinessCategoria;
       
        public CategoriaServices(ICategoriaRepository categoriaRepository, 
            IMapperCategoria mapperCategoria,
            IValidator<CreateCategoriaDto> createValidator, 
            IValidator<UpdateCategoriaDto> updateValidator,
            IValidatorBusinessCategoria validatorBusinessCategoria
            )
        {
            _categoriaRepository = categoriaRepository;
            _mapperCategoria = mapperCategoria;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _validatorBusinessCategoria = validatorBusinessCategoria;
        }


        public async Task<OperationResultD<CategoriaResponseDto>> CreateAsync(CreateCategoriaDto dto)
        {

            // Validación de datos
            var validationResult = await _createValidator.ValidateAsync(dto);
            
            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return OperationResultD<CategoriaResponseDto>.Failure($"Error de validación: {errors}");

            }
            // Validación de reglas de negocio

            var validationBusinessResult = await _validatorBusinessCategoria.ValidarCreateCategoriaAsync(dto);

            if (!validationBusinessResult.IsSuccess)
            {
                return OperationResultD<CategoriaResponseDto>.Failure(validationBusinessResult.Message);
            }


            var categoria = _mapperCategoria.MapToCreateCategoria(dto);

            await _categoriaRepository.CreateAsync(categoria);

            var categoriaResponsedto = _mapperCategoria.MapToCategoriaResponseDto(categoria);


            return OperationResultD<CategoriaResponseDto>.Success(categoriaResponsedto, "Categoria creada correctamente");
        }





        //no se usa por ahora
        public async Task<OperationResultD<bool>> DeleteAsync(int id)
        {
            if(id <=0)
            {
                return OperationResultD<bool>.Failure("El id no puede ser menor o igual a 0");
            }   

           
             var categoria = await _categoriaRepository.GetByIdAsync(id);

            if(categoria == null)
            {
                return OperationResultD<bool>.Failure("La categoria no fue encontrada");
            }
             await _categoriaRepository.DeleteAsync(id);

            return OperationResultD<bool>.Success(true, "Categoria eliminada correctamente");
        }





        public async Task<OperationResultD<bool>> DisableAsync(int id)
        {
            if (id <= 0)
            {
                return OperationResultD<bool>.Failure("El id no puede ser menor o igual a 0");

            }
          

            var categoria = await _categoriaRepository.GetByIdAsync(id);

            if (categoria == null)
            {
                return OperationResultD<bool>.Failure("La categoria no fue encontrada");
            }

            var validationBusinessResult = await _validatorBusinessCategoria.ValidarDeleteCategoriaAsync(categoria);
            if(!validationBusinessResult.IsSuccess)
            {
                return OperationResultD<bool>.Failure(validationBusinessResult.Message);
            }

            categoria.Desactivar();

            await _categoriaRepository.UpdateAsync(categoria);


            return OperationResultD<bool>.Success(true, "Categoria deshabilitada correctamente");
        }


        public async Task<OperationResultD<bool>> EnableCategoria(int id)
        {
            if (id <= 0)
            {
                return OperationResultD<bool>.Failure("El id no puede ser menor o igual a 0");
            }

            var categoria = await _categoriaRepository.GetByIdIncluyendoEliminadosAsync(id);

            if (categoria == null)
            {
                return OperationResultD<bool>.Failure("La categoria no fue encontrada");
            }

            categoria.Activar();

            await _categoriaRepository.UpdateAsync(categoria);

            return OperationResultD<bool>.Success(true, "Categoria activada correctamente");
        }






        public Task<OperationResultD<PagedResult<CategoriaResponseDto>>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
            => GetAllAsync(incluirInactivos: false, pageNumber, pageSize);

        public async Task<OperationResultD<PagedResult<CategoriaResponseDto>>> GetAllAsync(bool incluirInactivos, int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1)
                return OperationResultD<PagedResult<CategoriaResponseDto>>.Failure("pageNumber debe ser mayor o igual a 1");

            if (pageSize < 1 || pageSize > 100)
                return OperationResultD<PagedResult<CategoriaResponseDto>>.Failure("pageSize debe estar entre 1 y 100");

            var (categorias, totalCount) = await _categoriaRepository.GetPagedAsync(incluirInactivos, pageNumber, pageSize);

            var categoriaresponsedto = categorias.Select(c => _mapperCategoria.MapToCategoriaResponseDto(c)).ToList();

            var pagedResult = new PagedResult<CategoriaResponseDto>
            {
                Items = categoriaresponsedto,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return OperationResultD<PagedResult<CategoriaResponseDto>>.Success(pagedResult, "Categorias obtenidas correctamente");
        }



        public async Task<OperationResultD<CategoriaResponseDto>> GetByIdAsync(int id)
        {
           if(id <= 0)
             return OperationResultD<CategoriaResponseDto>.Failure("El id no puede ser menor o igual a 0");

            
             var categoria =  await _categoriaRepository.GetByIdAsync(id);
           
            if(categoria == null)
            {
                return OperationResultD<CategoriaResponseDto>.Failure("La categoria no fue encontrada");
            }
            var categoriaresponsedto = _mapperCategoria.MapToCategoriaResponseDto(categoria);


            return OperationResultD<CategoriaResponseDto>.Success(categoriaresponsedto, "Categoria obtenida correctamente");
        }




        public async Task<OperationResultD<CategoriaResponseDto>> UpdateAsync(UpdateCategoriaDto dto)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
                return OperationResultD<CategoriaResponseDto>.Failure($"Error de validación: {errors}");
            }

            var categoria = await _categoriaRepository.GetByIdAsync(dto.Id);
            if (categoria == null)
            {
                return OperationResultD<CategoriaResponseDto>.Failure("La categoria no fue encontrada");
            }

                var validationBusinessResult = await _validatorBusinessCategoria.ValidarUpdateCategoriaAsync(dto, categoria);
            if(!validationBusinessResult.IsSuccess)
            {
                return OperationResultD<CategoriaResponseDto>.Failure(validationBusinessResult.Message);
            }

            _mapperCategoria.MapToUpdateCategoria(dto, categoria);

            await _categoriaRepository.UpdateAsync(categoria);

            var categoriaResponsedto = _mapperCategoria.MapToCategoriaResponseDto(categoria);

            return OperationResultD<CategoriaResponseDto>.Success(categoriaResponsedto, "Categoria actualizada correctamente");

        }
    }
}
