using BarberOS.Application.Payments.DTOs;
using FluentValidation;

namespace BarberOS.Application.Payments.Validators
{
    public class RegisterPaymentRequestValidator : AbstractValidator<RegisterPaymentRequest>
    {
        public RegisterPaymentRequestValidator()
        {
            RuleFor(x => x.AppointmentId).NotEmpty();
            RuleFor(x => x.Method).IsInEnum();
            RuleFor(x => x.Amount)
                .GreaterThan(0).When(x => x.Amount.HasValue)
                .WithMessage("El monto debe ser mayor a cero.");
            RuleFor(x => x.Notes).MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Notes));
        }
    }
}
