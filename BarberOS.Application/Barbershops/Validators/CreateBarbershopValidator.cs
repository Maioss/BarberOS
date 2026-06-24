using BarberOS.Application.Barbershops.DTOs;
using FluentValidation;

namespace BarberOS.Application.Barbershops.Validators
{
    public class CreateBarbershopValidator : AbstractValidator<CreateBarbershopRequest>
    {
        public CreateBarbershopValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("El nombre es obligatorio.")
                .MaximumLength(150).WithMessage("El nombre no puede superar 150 caracteres.");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("La dirección es obligatoria.")
                .MaximumLength(300).WithMessage("La dirección no puede superar 300 caracteres.");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("La ciudad es obligatoria.")
                .MaximumLength(100).WithMessage("La ciudad no puede superar 100 caracteres.");

            RuleFor(x => x.Phone)
                .MaximumLength(20).WithMessage("El teléfono no puede superar 20 caracteres.")
                .When(x => x.Phone is not null);

            RuleFor(x => x.ParentId)
                .NotNull().WithMessage("Una sucursal requiere un ParentId.")
                .When(x => !x.IsMain);
        }
    }
}
