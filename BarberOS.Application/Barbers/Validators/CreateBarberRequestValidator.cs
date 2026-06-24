using BarberOS.Application.Barbers.DTOs;
using FluentValidation;

namespace BarberOS.Application.Barbers.Validators
{
    public class CreateBarberRequestValidator : AbstractValidator<CreateBarberRequest>
    {
        public CreateBarberRequestValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("El UserId es obligatorio.");
        }
    }
}
