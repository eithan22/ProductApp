using FluentValidation;
using ProductApp.Aplication.Dtos.Modulo_Configuracion;
using ProductApp.Aplication.Interface.IMappers.Modulo_Configuracion;
using ProductApp.Aplication.Interface.Servicios.Modulo_Configuracion;
using ProductApp.Aplication.Result.OperationResult;
using ProductApp.Domian.Interfaces;

namespace ProductApp.Aplication.Services.Modulo_Configuracion
{
    public class ConfiguracionSistemaService : IConfiguracionSistemaService
    {
        private readonly IConfiguracionSistemaRepository _configuracionSistemaRepository;
        private readonly IMapperConfiguracionSistema _mapperConfiguracionSistema;
        private readonly IValidator<ActualizarConfiguracionSistemaDto> _actualizarValidator;

        public ConfiguracionSistemaService(
            IConfiguracionSistemaRepository configuracionSistemaRepository,
            IMapperConfiguracionSistema mapperConfiguracionSistema,
            IValidator<ActualizarConfiguracionSistemaDto> actualizarValidator)
        {
            _configuracionSistemaRepository = configuracionSistemaRepository;
            _mapperConfiguracionSistema = mapperConfiguracionSistema;
            _actualizarValidator = actualizarValidator;
        }

        public async Task<OperationResultD<ConfiguracionSistemaDto>> ObtenerAsync()
        {
            var configuracion = await _configuracionSistemaRepository.ObtenerAsync();
            if (configuracion == null)
                return OperationResultD<ConfiguracionSistemaDto>.Failure("Configuración no encontrada");

            return OperationResultD<ConfiguracionSistemaDto>.Success(
                _mapperConfiguracionSistema.ToDto(configuracion), "Configuración obtenida correctamente");
        }

        public async Task<OperationResultD<ConfiguracionSistemaDto>> ActualizarAsync(ActualizarConfiguracionSistemaDto dto)
        {
            var validationResult = await _actualizarValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return OperationResultD<ConfiguracionSistemaDto>.Failure($"Validación fallida: {errors}");
            }

            var configuracion = await _configuracionSistemaRepository.ObtenerAsync();
            if (configuracion == null)
                return OperationResultD<ConfiguracionSistemaDto>.Failure("Configuración no encontrada");

            configuracion.ActualizarParametros(
                dto.CantidadMinimaInventarioDefecto,
                dto.DuracionTokenMinutos,
                dto.NombreEmpresa,
                dto.Moneda);

            await _configuracionSistemaRepository.ActualizarAsync(configuracion);

            return OperationResultD<ConfiguracionSistemaDto>.Success(
                _mapperConfiguracionSistema.ToDto(configuracion), "Configuración actualizada correctamente");
        }
    }
}
