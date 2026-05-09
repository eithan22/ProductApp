using FluentValidation;
using ProductApp.Aplication.Dtos.Modulo_Usuarios.UsuarioDto.AuthDto;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace ProductApp.Aplication.Validators.Modulo_Usuario.AuthValidator;

    public class RegisterValidator : AbstractValidator<RegisteDto>
    {
        public RegisterValidator() 
        {

            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es requerido.")
                .MaximumLength(30).WithMessage("El nombre no puede exceder los 30 caracteres.")
                .Matches(@"^[a-zA-Z\s]+$").WithMessage("El nombre solo puede contener letras y espacios.");


        RuleFor(x => x.Edad)
                .GreaterThan(0).WithMessage("La edad debe ser mayor a 0.")
                .LessThanOrEqualTo(100).WithMessage("La edad no puede ser mayor a 100.");



        RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido.")
                .EmailAddress().WithMessage("El email no es válido.")
                .Empty().WithMessage("El email no puede estar vacío.")
                .MaximumLength(30).WithMessage("El email no puede exceder los 30 caracteres.");





        RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es requerida.")
                .MinimumLength(6).WithMessage("La contraseña debe tener al menos 6 caracteres.")
                .MaximumLength(20).WithMessage("La contraseña no puede exceder los 20 caracteres.")
                .Matches(@"[A-Z]").WithMessage("La contraseña debe contener al menos una letra mayúscula.")
                .Matches(@"[a-z]").WithMessage("La contraseña debe contener al menos una letra minúscula.")
                .Matches(@"[0-9]").WithMessage("La contraseña debe contener al menos un número.");
          


        RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("La confirmación de contraseña es requerida.")
                .Equal(x => x.Password).WithMessage("Las contraseñas no coinciden.");


        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("El nombre de usuario es requerido.")
            .MaximumLength(20).WithMessage("El nombre de usuario no puede exceder los 20 caracteres.")
            .Matches(@"^[a-zA-Z0-9]+$").WithMessage("El nombre de usuario solo puede contener letras y números.");
            














    }
    }

