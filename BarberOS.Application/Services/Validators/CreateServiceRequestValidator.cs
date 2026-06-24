using BarberOS.Application.Services.DTOs;
using FluentValidation;

namespace BarberOS.Application.Services.Validators
{
    public class CreateServiceRequestValidator : AbstractValidator<CreateServiceRequest>
    {
        public CreateServiceRequestValidator()
        {
            RuleFor(x => x.BarbershopId)
                .NotEmpty().WithMessage("La barbería es obligatoria.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre del servicio es obligatorio.")
                .MinimumLength(2).MaximumLength(80);

            RuleFor(x => x.Description)
                .MaximumLength(300)
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleFor(x => x.Price)
                .GreaterThanOrEqualTo(0).WithMessage("El precio no puede ser negativo.");

            RuleFor(x => x.DurationMinutes)
                .GreaterThan(0).WithMessage("La duración debe ser mayor a cero.")
                .LessThanOrEqualTo(240).WithMessage("La duración no puede exceder 4 horas.");
        }
    }
}
