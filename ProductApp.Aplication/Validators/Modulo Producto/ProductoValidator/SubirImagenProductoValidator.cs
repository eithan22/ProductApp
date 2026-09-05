using FluentValidation;
using ProductApp.Aplication.Dtos.ProductoDto;

namespace ProductApp.Aplication.Validators.Modulo_Producto.ProductoValidator
{
    public class SubirImagenProductoValidator : AbstractValidator<SubirImagenProductoDto>
    {
        public const long TamanoMaximoBytes = 5 * 1024 * 1024;

        private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };
        private static readonly string[] ContentTypesPermitidos = { "image/jpeg", "image/png", "image/webp" };

        public SubirImagenProductoValidator()
        {
            RuleFor(x => x.ProductoId)
                .GreaterThan(0).WithMessage("El id del producto es inválido.");

            RuleFor(x => x.Contenido)
                .NotNull().WithMessage("Debe adjuntar un archivo de imagen.");

            RuleFor(x => x.NombreArchivo)
                .NotEmpty().WithMessage("El nombre del archivo es requerido.")
                .Must(TieneExtensionPermitida)
                .WithMessage($"La extensión del archivo no está permitida. Permitidas: {string.Join(", ", ExtensionesPermitidas)}.");

            RuleFor(x => x.ContentType)
                .NotEmpty().WithMessage("No se pudo determinar el tipo del archivo.")
                .Must(ct => ContentTypesPermitidos.Contains(ct.ToLowerInvariant()))
                .WithMessage($"El tipo de archivo no está permitido. Permitidos: {string.Join(", ", ContentTypesPermitidos)}.");

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
