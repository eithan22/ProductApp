using FluentValidation;
using ProductApp.Aplication.Common;
using ProductApp.Aplication.Dtos.Modulo_Proveedores.ProveedorDto;
using ProductApp.Aplication.Interface;
using ProductApp.Aplication.Interface.IMappers.Modulo_Proveedores;
using ProductApp.Aplication.Interface.RulesBusinnes.Modulo_Proveedores;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Interfaces;

namespace ProductApp.Aplication.Services
{
    public class ProveedorService : IProveedorServices
    {
        private readonly IProveedorRepository _proveedorRepository;
        private readonly IMapperProveedor _mapperProveedor;
        private readonly IValidator<CreateProveedorDto> _createValidator;
        private readonly IValidator<UpdateProveedorDto> _updateValidator;
        private readonly IValidatorBusinessProveedor _validatorBusinessProveedor;

        public ProveedorService(
            IProveedorRepository proveedorRepository,
            IMapperProveedor mapperProveedor,
            IValidator<CreateProveedorDto> createValidator,
            IValidator<UpdateProveedorDto> updateValidator,
            IValidatorBusinessProveedor validatorBusinessProveedor)
        {
            _proveedorRepository = proveedorRepository;
            _mapperProveedor = mapperProveedor;
            _createValidator = createValidator;
            _updateValidator = updateValidator;
            _validatorBusinessProveedor = validatorBusinessProveedor;
        }

        public async Task<OperationResultD<ProveedorResponseDto>> CreateAsync(CreateProveedorDto dto)
        {
            var validationResult = await _createValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return OperationResultD<ProveedorResponseDto>.Failure($"Error de validación: {errors}");
            }

            var businessValidationResult = await _validatorBusinessProveedor.ValidarCreateProveedorAsync(dto);
            if (!businessValidationResult.IsSuccess)
            {
                return OperationResultD<ProveedorResponseDto>.Failure(businessValidationResult.Message);
            }

            var proveedor = _mapperProveedor.MapToCreateProveedor(dto);

            await _proveedorRepository.CreateAsync(proveedor);

            var proveedorResponse = _mapperProveedor.MapToProveedorResponseDto(proveedor);

            return OperationResultD<ProveedorResponseDto>.Success(proveedorResponse, "Proveedor creado correctamente");
        }

        public Task<OperationResultD<PagedResult<ProveedorResponseDto>>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
            => GetAllAsync(incluirInactivos: false, pageNumber, pageSize);

        public async Task<OperationResultD<PagedResult<ProveedorResponseDto>>> GetAllAsync(bool incluirInactivos, int pageNumber = 1, int pageSize = 10)
        {
            if (pageNumber < 1)
                return OperationResultD<PagedResult<ProveedorResponseDto>>.Failure("pageNumber debe ser mayor o igual a 1");

            if (pageSize < 1 || pageSize > 100)
                return OperationResultD<PagedResult<ProveedorResponseDto>>.Failure("pageSize debe estar entre 1 y 100");

            var (proveedores, totalCount) = await _proveedorRepository.GetAllProveedoresAsync(incluirInactivos, pageNumber, pageSize);

            var proveedoresResponseDto = proveedores
                .Select(p => _mapperProveedor.MapToProveedorResponseDto(p))
                .ToList();

            var pagedResult = new PagedResult<ProveedorResponseDto>
            {
                Items = proveedoresResponseDto,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return OperationResultD<PagedResult<ProveedorResponseDto>>.Success(pagedResult, "Proveedores obtenidos correctamente");
        }

        public async Task<OperationResultD<ProveedorResponseDto>> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                return OperationResultD<ProveedorResponseDto>.Failure("El id no puede ser menor o igual a 0");
            }

            var proveedor = await _proveedorRepository.GetByIdAsync(id);
            if (proveedor == null)
            {
                return OperationResultD<ProveedorResponseDto>.Failure("El proveedor no fue encontrado");
            }

            var proveedorResponseDto = _mapperProveedor.MapToProveedorResponseDto(proveedor);

            return OperationResultD<ProveedorResponseDto>.Success(proveedorResponseDto, "Proveedor obtenido correctamente");
        }

        public async Task<OperationResultD<ProveedorResponseDto>> UpdateAsync(UpdateProveedorDto dto)
        {
            var validationResult = await _updateValidator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return OperationResultD<ProveedorResponseDto>.Failure($"Error de validación: {errors}");
            }

            var proveedor = await _proveedorRepository.GetByIdAsync(dto.Id);
            if (proveedor == null)
            {
                return OperationResultD<ProveedorResponseDto>.Failure("El proveedor no fue encontrado");
            }

            var validationResultBusiness = await _validatorBusinessProveedor.ValidarUpdateProveedorAsync(dto, proveedor);
            if (!validationResultBusiness.IsSuccess)
            {
                return OperationResultD<ProveedorResponseDto>.Failure(validationResultBusiness.Message);
            }

            _mapperProveedor.MapToUpdateProveedor(dto, proveedor);

            await _proveedorRepository.UpdateAsync(proveedor);

            var proveedorResponseDto = _mapperProveedor.MapToProveedorResponseDto(proveedor);

            return OperationResultD<ProveedorResponseDto>.Success(proveedorResponseDto, "Proveedor actualizado correctamente");
        }

        // Delete físico. No está expuesto en el controller a propósito: la baja que usa la
        // aplicación es DisableAsync. Se implementa porque IBaseServices lo exige y para
        // dejar lista la operación con su regla de negocio si más adelante hace falta.
        public async Task<OperationResultD<bool>> DeleteAsync(int id)
        {
            if (id <= 0)
            {
                return OperationResultD<bool>.Failure("El id no puede ser menor o igual a 0");
            }

            var proveedor = await _proveedorRepository.GetByIdAsync(id);
            if (proveedor == null)
            {
                return OperationResultD<bool>.Failure("El proveedor no fue encontrado");
            }

            var validationResult = await _validatorBusinessProveedor.ValidarBorradoFisicoProveedorAsync(proveedor);
            if (!validationResult.IsSuccess)
            {
                return OperationResultD<bool>.Failure(validationResult.Message);
            }

            await _proveedorRepository.DeleteAsync(id);

            return OperationResultD<bool>.Success(true, "Proveedor eliminado correctamente");
        }

        // Delete lógico (desactivación): es la baja real que usa la aplicación.
        public async Task<OperationResultD<bool>> DisableAsync(int id)
        {
            if (id <= 0)
            {
                return OperationResultD<bool>.Failure("El id no puede ser menor o igual a 0");
            }

            var proveedor = await _proveedorRepository.GetByIdAsync(id);
            if (proveedor == null)
            {
                return OperationResultD<bool>.Failure("El proveedor no fue encontrado");
            }

            var validationResult = await _validatorBusinessProveedor.ValidarDeleteProveedorAsync(proveedor);
            if (!validationResult.IsSuccess)
            {
                return OperationResultD<bool>.Failure(validationResult.Message);
            }

            proveedor.Desactivar();

            await _proveedorRepository.UpdateAsync(proveedor);

            return OperationResultD<bool>.Success(true, "Proveedor desactivado correctamente");
        }

        public async Task<OperationResultD<bool>> EnableProveedor(int id)
        {
            if (id <= 0)
            {
                return OperationResultD<bool>.Failure("El id no puede ser menor o igual a 0");
            }

            var proveedor = await _proveedorRepository.GetByIdAsync(id);
            if (proveedor == null)
            {
                return OperationResultD<bool>.Failure("El proveedor no fue encontrado");
            }

            proveedor.Activar();

            await _proveedorRepository.UpdateAsync(proveedor);

            return OperationResultD<bool>.Success(true, "Proveedor activado correctamente");
        }

        public async Task<OperationResultD<List<ProveedorResponseDto>>> BuscarAsync(string? nombre, bool incluirInactivos = false)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return OperationResultD<List<ProveedorResponseDto>>.Failure("Debe proporcionar al menos un criterio de búsqueda");
            }

            var proveedores = await _proveedorRepository.BuscarProveedoresAsync(nombre, incluirInactivos);

            var proveedoresResponseDto = proveedores
                .Select(p => _mapperProveedor.MapToProveedorResponseDto(p))
                .ToList();

            return OperationResultD<List<ProveedorResponseDto>>.Success(proveedoresResponseDto, "Proveedores obtenidos correctamente");
        }
    }
}
