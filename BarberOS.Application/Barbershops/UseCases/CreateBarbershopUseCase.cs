using BarberOS.Application.Barbershops.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Entities;
using BarberOS.Domain.Exceptions;
using FluentValidation;

namespace BarberOS.Application.Barbershops.UseCases
{
    public class CreateBarbershopUseCase
    {
        private readonly IBarbershopRepository _repo;
        private readonly IUnitOfWork _uow;
        private readonly IValidator<CreateBarbershopRequest> _validator;

        public CreateBarbershopUseCase(
            IBarbershopRepository repo,
            IUnitOfWork uow,
            IValidator<CreateBarbershopRequest> validator)
        {
            _repo = repo;
            _uow = uow;
            _validator = validator;
        }

        public async Task<BarbershopDto> ExecuteAsync(CreateBarbershopRequest request, CancellationToken ct = default)
        {
            await _validator.ValidateAndThrowAsync(request, ct);

            Barbershop barbershop;

            if (request.IsMain)
            {
                barbershop = Barbershop.CreateMain(request.Name, request.Address, request.City, request.Phone);
            }
            else
            {
                if (request.ParentId is null)
                    throw new BusinessRuleException("Una sucursal debe tener una sede principal asociada (ParentId).");

                var parent = await _repo.GetByIdAsync(request.ParentId.Value, ct)
                    ?? throw NotFoundException.For("Barbershop (sede principal)", request.ParentId.Value);

                if (!parent.IsMain)
                    throw new BusinessRuleException("El ParentId debe corresponder a una sede principal.");

                barbershop = Barbershop.CreateBranch(request.Name, request.Address, request.City, request.Phone, request.ParentId.Value);
            }

            await _repo.AddAsync(barbershop, ct);
            await _uow.SaveChangesAsync(ct);

            return new BarbershopDto(
                barbershop.Id, barbershop.Name, barbershop.Address, barbershop.City, barbershop.Phone,
                barbershop.IsMain, barbershop.ParentId, barbershop.IsActive, barbershop.CreatedAt, barbershop.UpdatedAt
            );
        }
    }
}
