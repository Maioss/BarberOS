using BarberOS.Application.Auth.DTOs;
using FluentValidation;

namespace BarberOS.Application.Auth.Validators
{

    public class RegisterClientRequestValidator : AbstractValidator<RegisterClientRequest>
    {
        public RegisterClientRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El correo es obligatorio.")
                .EmailAddress().WithMessage("El correo no tiene un formato válido.")
                .MaximumLength(160);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es obligatoria.")
                .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
                .Matches(@"[A-Z]").WithMessage("La contraseña debe incluir al menos una mayúscula.")
                .Matches(@"[a-z]").WithMessage("La contraseña debe incluir al menos una minúscula.")
                .Matches(@"\d").WithMessage("La contraseña debe incluir al menos un número.");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("El nombre es obligatorio.")
                .MinimumLength(2).MaximumLength(120);

            RuleFor(x => x.Phone)
                .MaximumLength(30)
                .When(x => !string.IsNullOrWhiteSpace(x.Phone));
        }
    }
}
