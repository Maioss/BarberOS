using BarberOS.Application.Appointments.DTOs;
using FluentValidation;

namespace BarberOS.Application.Appointments.Validators
{
    public class CreateAppointmentRequestValidator : AbstractValidator<CreateAppointmentRequest>
    {
        public CreateAppointmentRequestValidator()
        {
            RuleFor(x => x.BarberId).NotEmpty();
            RuleFor(x => x.Date).NotEmpty();
            RuleFor(x => x.StartTime).NotEmpty();
            RuleFor(x => x.ServiceIds)
                .NotEmpty().WithMessage("Debe incluir al menos un servicio.")
                .Must(ids => ids.All(id => id != Guid.Empty)).WithMessage("Los IDs de servicios no pueden ser vacíos.");
            RuleFor(x => x.Notes).MaximumLength(500).When(x => !string.IsNullOrWhiteSpace(x.Notes));
        }
    }
}
