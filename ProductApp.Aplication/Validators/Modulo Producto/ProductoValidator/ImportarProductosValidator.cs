using FluentValidation;
using ProductApp.Aplication.Dtos.Modulo_Productos.ImportacionDto;

namespace ProductApp.Aplication.Validators.Modulo_Producto.ProductoValidator
{
    public class ImportarProductosValidator : AbstractValidator<ImportarProductosDto>
    {
        // 5 MB, el mismo tope que anuncia la pantalla de importación.
        public const long TamanoMaximoBytes = 5 * 1024 * 1024;

        private static readonly string[] ExtensionesPermitidas = { ".xlsx", ".csv" };

        public ImportarProductosValidator()
        {
            RuleFor(x => x.Contenido)
                .NotNull().WithMessage("Debe adjuntar un archivo.");

            RuleFor(x => x.NombreArchivo)
                .NotEmpty().WithMessage("El nombre del archivo es requerido.")
                .Must(TieneExtensionPermitida)
                .WithMessage($"La extensión del archivo no está permitida. Permitidas: {string.Join(", ", ExtensionesPermitidas)}.");

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
