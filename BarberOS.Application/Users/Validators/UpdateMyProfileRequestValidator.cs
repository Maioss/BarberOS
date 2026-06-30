using BarberOS.Application.Users.DTOs;
using FluentValidation;

namespace BarberOS.Application.Users.Validators;

public class UpdateMyProfileRequestValidator : AbstractValidator<UpdateMyProfileRequest>
{
    public UpdateMyProfileRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MinimumLength(2).MaximumLength(120);
        RuleFor(x => x.Phone).MaximumLength(30).When(x => !string.IsNullOrWhiteSpace(x.Phone));
    }
}
