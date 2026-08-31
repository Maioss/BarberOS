using BarberOS.Domain.Entities;

namespace BarberOS.Application.Shared
{
    public interface IBalanceEntryRepository
    {
        Task AddAsync(BalanceEntry entry, CancellationToken ct = default);

        Task<decimal> GetBalanceAsync(Guid barberId, CancellationToken ct = default);

        Task<IReadOnlyList<BalanceEntry>> ListByBarberAsync(Guid barberId, CancellationToken ct = default);
    }
}
