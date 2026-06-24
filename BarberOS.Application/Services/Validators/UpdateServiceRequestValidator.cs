using BarberOS.Application.Services.DTOs;
using FluentValidation;

namespace BarberOS.Application.Services.Validators
{
    public class UpdateServiceRequestValidator : AbstractValidator<UpdateServiceRequest>
    {
        public UpdateServiceRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().MinimumLength(2).MaximumLength(80);

            RuleFor(x => x.Description)
                .MaximumLength(300)
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleFor(x => x.Price).GreaterThanOrEqualTo(0);

            RuleFor(x => x.DurationMinutes)
                .GreaterThan(0).LessThanOrEqualTo(240);
        }
    }
}
