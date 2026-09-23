using FluentValidation;
using ProductApp.Aplication.Dtos.Modulo_Configuracion;

namespace ProductApp.Aplication.Validators.Modulo_Configuracion
{
    public class SubirLogoEmpresaValidator : AbstractValidator<SubirLogoEmpresaDto>
    {
        public const long TamanoMaximoBytes = 5 * 1024 * 1024;

        private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };

        public SubirLogoEmpresaValidator()
        {
            RuleFor(x => x.Contenido)
                .NotNull().WithMessage("Debe adjuntar un archivo de imagen.");

            RuleFor(x => x.NombreArchivo)
                .NotEmpty().WithMessage("El nombre del archivo es requerido.")
                .Must(TieneExtensionPermitida)
                .WithMessage($"La extensión del archivo no está permitida. Permitidas: {string.Join(", ", ExtensionesPermitidas)}.");

            // El Content-Type declarado por el cliente ya no se valida acá: no es confiable y
            // rechazaba clientes legítimos que envían application/octet-stream. Quien decide el
            // tipo real es DetectorFormatoImagen, sobre la firma binaria, en el servicio.

            RuleFor(x => x.TamanoBytes)
                .GreaterThan(0).WithMessage("El archivo está vacío.")
                .LessThanOrEqualTo(TamanoMaximoBytes)
                .WithMessage($"El archivo no puede superar los {TamanoMaximoBytes / (1024 * 1024)} MB.");
        }

        private static bool TieneExtensionPermitida(string nombreArchivo)
        {
            var extension = Path.GetExtension(nombreArchivo ?? string.Empty).ToLowerInvariant();
            return ExtensionesPermitidas.Contains(extension);
        }
    }
}
