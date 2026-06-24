using BarberOS.Application.Barbershops.DTOs;
using BarberOS.Domain.Entities;

namespace BarberOS.Application.Shared
{
    public interface IBarbershopRepository
    {
        Task<Barbershop?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<PagedResult<Barbershop>> ListAsync(BarbershopFilter filter, CancellationToken ct = default);
        Task<IReadOnlyList<Barbershop>> ListBranchesAsync(Guid parentId, CancellationToken ct = default);
        Task<bool> HasActiveBranchesAsync(Guid parentId, CancellationToken ct = default);
        Task AddAsync(Barbershop barbershop, CancellationToken ct = default);
        void Update(Barbershop barbershop);
        void Remove(Barbershop barbershop);
    }
}
