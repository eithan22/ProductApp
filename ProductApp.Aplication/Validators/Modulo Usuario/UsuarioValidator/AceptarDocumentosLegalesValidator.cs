using FluentValidation;
using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto;
using ProductApp.Domian.Entitis;

namespace ProductApp.Aplication.Validators.Modulo_Usuario.UsuarioValidator
{
    public class AceptarDocumentosLegalesValidator : AbstractValidator<AceptarDocumentosLegalesDto>
    {
        public AceptarDocumentosLegalesValidator()
        {
            RuleFor(x => x.Version)
                .NotEmpty().WithMessage("La versión de los documentos es requerida.")
                .MaximumLength(Usuario.LargoMaximoVersionDocumentosLegales)
                    .WithMessage($"La versión no puede exceder los {Usuario.LargoMaximoVersionDocumentosLegales} caracteres.");
        }
    }
}
