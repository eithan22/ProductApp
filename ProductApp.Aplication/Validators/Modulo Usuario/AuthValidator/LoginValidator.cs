using FluentValidation;
using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto.AuthDto;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductApp.Aplication.Validators.Modulo_Usuario.AuthValidator
{
    public class LoginValidator : AbstractValidator<LoginDto>
    {
        public LoginValidator() 
        {

            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("El nombre de usuario es requerido.");


            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es requerida.");
                









        }
    }
}
