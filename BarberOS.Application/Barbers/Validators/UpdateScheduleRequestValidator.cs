using BarberOS.Application.Barbers.DTOs;
using FluentValidation;

namespace BarberOS.Application.Barbers.Validators
{
    public class UpdateScheduleRequestValidator : AbstractValidator<UpdateScheduleRequest>
    {
        public UpdateScheduleRequestValidator()
        {
            RuleFor(x => x.LunchStart)
                .NotEmpty().WithMessage("La hora de inicio del almuerzo es obligatoria.");

            RuleFor(x => x.LunchEnd)
                .NotEmpty().WithMessage("La hora de fin del almuerzo es obligatoria.");

            RuleFor(x => x.AvailableDays)
                .NotEmpty().WithMessage("Debes indicar al menos un día disponible.")
                .Must(days => days.Distinct().Count() == days.Count)
                .WithMessage("Hay días duplicados en la lista.");
        }
    }
}
