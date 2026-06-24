using BarberOS.Application.Barbershops.DTOs;
using BarberOS.Application.Shared;

namespace BarberOS.Application.Barbershops.UseCases
{
    public class ListBarbershopsUseCase
    {
        private readonly IBarbershopRepository _repo;

        public ListBarbershopsUseCase(IBarbershopRepository repo)
        {
            _repo = repo;
        }

        public async Task<PagedResult<BarbershopDto>> ExecuteAsync(BarbershopFilter filter, CancellationToken ct = default)
        {
            var paged = await _repo.ListAsync(filter, ct);
            var items = paged.Items.Select(b => new BarbershopDto(
                b.Id, b.Name, b.Address, b.City, b.Phone,
                b.IsMain, b.ParentId, b.IsActive, b.CreatedAt, b.UpdatedAt
            )).ToList();
            return new PagedResult<BarbershopDto>(items, paged.Page, paged.PageSize, paged.TotalCount);
        }
    }
}
