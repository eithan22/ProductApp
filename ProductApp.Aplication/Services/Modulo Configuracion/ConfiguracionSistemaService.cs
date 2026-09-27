using FluentValidation;
using Microsoft.Extensions.Logging;
using ProductApp.Aplication.Common;
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
        private readonly IValidator<SubirLogoEmpresaDto> _subirLogoValidator;
        private readonly IAlmacenamientoImagenes _almacenamientoImagenes;
        private readonly ILogger<ConfiguracionSistemaService> _logger;

        public ConfiguracionSistemaService(
            IConfiguracionSistemaRepository configuracionSistemaRepository,
            IMapperConfiguracionSistema mapperConfiguracionSistema,
            IValidator<ActualizarConfiguracionSistemaDto> actualizarValidator,
            IValidator<SubirLogoEmpresaDto> subirLogoValidator,
            IAlmacenamientoImagenes almacenamientoImagenes,
            ILogger<ConfiguracionSistemaService> logger)
        {
            _configuracionSistemaRepository = configuracionSistemaRepository;
            _mapperConfiguracionSistema = mapperConfiguracionSistema;
            _actualizarValidator = actualizarValidator;
            _subirLogoValidator = subirLogoValidator;
            _almacenamientoImagenes = almacenamientoImagenes;
            _logger = logger;
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
                dto.Moneda,
                dto.RucONit,
                dto.Direccion);

            await _configuracionSistemaRepository.ActualizarAsync(configuracion);

            return OperationResultD<ConfiguracionSistemaDto>.Success(
                _mapperConfiguracionSistema.ToDto(configuracion), "Configuración actualizada correctamente");
        }

        public async Task<OperationResultD<ConfiguracionSistemaDto>> SubirLogoAsync(SubirLogoEmpresaDto dto)
        {
            var validationResult = await _subirLogoValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                return OperationResultD<ConfiguracionSistemaDto>.Failure($"Validación fallida: {errors}");
            }

            // Mismo criterio que la imagen de producto: manda la firma del archivo, no lo que
            // declaró el cliente en el nombre o el Content-Type.
            var formato = await DetectorFormatoImagen.DetectarAsync(dto.Contenido);

            if (formato is null)
            {
                _logger.LogWarning(
                    "Se rechazó el logo {NombreArchivo}: el contenido no corresponde a JPEG, PNG ni WEBP",
                    dto.NombreArchivo);

                return OperationResultD<ConfiguracionSistemaDto>.Failure(
                    "El contenido del archivo no es una imagen válida. Se aceptan JPEG, PNG y WEBP.");
            }

            var configuracion = await _configuracionSistemaRepository.ObtenerAsync();
            if (configuracion == null)
                return OperationResultD<ConfiguracionSistemaDto>.Failure("Configuración no encontrada");

            var logoAnterior = configuracion.LogoUrl;

            var nombreNormalizado = Path.ChangeExtension(dto.NombreArchivo, formato.Extension);

            var nuevaUrl = await _almacenamientoImagenes.SubirAsync(dto.Contenido, nombreNormalizado, formato.ContentType);

            configuracion.AsignarLogo(nuevaUrl);

            await _configuracionSistemaRepository.ActualizarAsync(configuracion);

            // El blob viejo se borra recién después de persistir la nueva url: si la subida o el
            // guardado fallan, la configuración conserva un logo que sigue existiendo en el storage.
            if (!string.IsNullOrWhiteSpace(logoAnterior) && logoAnterior != nuevaUrl)
            {
                try
                {
                    await _almacenamientoImagenes.EliminarAsync(logoAnterior);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "No se pudo eliminar el logo anterior {LogoAnterior} de la configuración del sistema",
                        logoAnterior);
                }
            }

            return OperationResultD<ConfiguracionSistemaDto>.Success(
                _mapperConfiguracionSistema.ToDto(configuracion), "Logo de la empresa actualizado correctamente");
        }

        public async Task<OperationResultD<ConfiguracionSistemaDto>> QuitarLogoAsync()
        {
            var configuracion = await _configuracionSistemaRepository.ObtenerAsync();
            if (configuracion == null)
                return OperationResultD<ConfiguracionSistemaDto>.Failure("Configuración no encontrada");

            var logoAnterior = configuracion.LogoUrl;
            if (string.IsNullOrWhiteSpace(logoAnterior))
                return OperationResultD<ConfiguracionSistemaDto>.Failure("La configuración no tiene un logo cargado");

            configuracion.QuitarLogo();
            await _configuracionSistemaRepository.ActualizarAsync(configuracion);

            try
            {
                await _almacenamientoImagenes.EliminarAsync(logoAnterior);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo eliminar el logo {LogoAnterior} del storage", logoAnterior);
            }

            return OperationResultD<ConfiguracionSistemaDto>.Success(
                _mapperConfiguracionSistema.ToDto(configuracion), "Logo de la empresa eliminado correctamente");
        }
    }
}
