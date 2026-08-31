using BarberOS.Application.Shared;
using BarberOS.Domain.Entities;
using BarberOS.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace BarberOS.Infrastructure.Persistence.Repositories
{
    public class BalanceEntryRepository : IBalanceEntryRepository
    {
        private readonly BarberOSDbContext _db;

        public BalanceEntryRepository(BarberOSDbContext db) => _db = db;

        public async Task AddAsync(BalanceEntry entry, CancellationToken ct = default)
        {
            await _db.BalanceEntries.AddAsync(BalanceEntryMapper.ToDbModel(entry), ct);
        }

        public async Task<decimal> GetBalanceAsync(Guid barberId, CancellationToken ct = default)
        {
            return await _db.BalanceEntries.AsNoTracking()
                .Where(e => e.BarberId == barberId)
                .SumAsync(e => (decimal?)e.Amount, ct) ?? 0m;
        }

        public async Task<IReadOnlyList<BalanceEntry>> ListByBarberAsync(Guid barberId, CancellationToken ct = default)
        {
            var rows = await _db.BalanceEntries.AsNoTracking()
                .Where(e => e.BarberId == barberId)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync(ct);

            return rows.Select(BalanceEntryMapper.ToDomain).ToList();
        }
    }
}
