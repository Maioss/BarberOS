using BarberOS.Application.Users.DTOs;
using FluentValidation;

namespace BarberOS.Application.Users.Validators
{
    public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
    {
        public UpdateUserRequestValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("El nombre es obligatorio.")
                .MinimumLength(2).MaximumLength(120);

            RuleFor(x => x.Phone)
                .MaximumLength(30)
                .When(x => !string.IsNullOrWhiteSpace(x.Phone));

            RuleFor(x => x.Role)
                .IsInEnum().WithMessage("Rol inválido.");
        }
    }
}
