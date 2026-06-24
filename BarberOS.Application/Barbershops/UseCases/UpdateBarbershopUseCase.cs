using BarberOS.Application.Barbershops.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Exceptions;
using FluentValidation;

namespace BarberOS.Application.Barbershops.UseCases
{
    public class UpdateBarbershopUseCase
    {
        private readonly IBarbershopRepository _repo;
        private readonly IUnitOfWork _uow;
        private readonly IValidator<UpdateBarbershopRequest> _validator;

        public UpdateBarbershopUseCase(
            IBarbershopRepository repo,
            IUnitOfWork uow,
            IValidator<UpdateBarbershopRequest> validator)
        {
            _repo = repo;
            _uow = uow;
            _validator = validator;
        }

        public async Task<BarbershopDto> ExecuteAsync(Guid id, UpdateBarbershopRequest request, CancellationToken ct = default)
        {
            await _validator.ValidateAndThrowAsync(request, ct);

            var barbershop = await _repo.GetByIdAsync(id, ct)
                ?? throw NotFoundException.For("Barbershop", id);

            barbershop.UpdateInfo(request.Name, request.Address, request.City, request.Phone);
            _repo.Update(barbershop);
            await _uow.SaveChangesAsync(ct);

            return new BarbershopDto(
                barbershop.Id, barbershop.Name, barbershop.Address, barbershop.City, barbershop.Phone,
                barbershop.IsMain, barbershop.ParentId, barbershop.IsActive, barbershop.CreatedAt, barbershop.UpdatedAt
            );
        }
    }
}
