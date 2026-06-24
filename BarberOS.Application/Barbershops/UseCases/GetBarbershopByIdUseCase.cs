using BarberOS.Application.Barbershops.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Barbershops.UseCases
{
    public class GetBarbershopByIdUseCase
    {
        private readonly IBarbershopRepository _repo;

        public GetBarbershopByIdUseCase(IBarbershopRepository repo)
        {
            _repo = repo;
        }

        public async Task<BarbershopDto> ExecuteAsync(Guid id, CancellationToken ct = default)
        {
            var b = await _repo.GetByIdAsync(id, ct)
                ?? throw NotFoundException.For("Barbershop", id);
            return new BarbershopDto(
                b.Id, b.Name, b.Address, b.City, b.Phone,
                b.IsMain, b.ParentId, b.IsActive, b.CreatedAt, b.UpdatedAt
            );
        }
    }
}
