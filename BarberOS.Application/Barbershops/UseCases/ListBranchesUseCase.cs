using BarberOS.Application.Barbershops.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Barbershops.UseCases
{
    public class ListBranchesUseCase
    {
        private readonly IBarbershopRepository _repo;

        public ListBranchesUseCase(IBarbershopRepository repo)
        {
            _repo = repo;
        }

        public async Task<IReadOnlyList<BarbershopDto>> ExecuteAsync(Guid parentId, CancellationToken ct = default)
        {
            var parent = await _repo.GetByIdAsync(parentId, ct)
                ?? throw NotFoundException.For("Barbershop", parentId);

            if (!parent.IsMain)
                throw new BusinessRuleException("Solo se pueden listar sucursales de una sede principal.");

            var branches = await _repo.ListBranchesAsync(parentId, ct);
            return branches.Select(b => new BarbershopDto(
                b.Id, b.Name, b.Address, b.City, b.Phone,
                b.IsMain, b.ParentId, b.IsActive, b.CreatedAt, b.UpdatedAt
            )).ToList();
        }
    }
}
