using BarberOS.Application.Shared;
using BarberOS.Domain.Entities;
using BarberOS.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace BarberOS.Infrastructure.Persistence.Repositories
{
    public class BarberRepository : IBarberRepository
    {
        private readonly BarberOSDbContext _db;

        public BarberRepository(BarberOSDbContext db) => _db = db;

        public async Task<Barber?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var row = await _db.Barbers.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
            return row is null ? null : BarberMapper.ToDomain(row);
        }

        public async Task<Barber?> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
        {
            var row = await _db.Barbers.AsNoTracking().FirstOrDefaultAsync(x => x.UserId == userId, ct);
            return row is null ? null : BarberMapper.ToDomain(row);
        }

        public Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken ct = default) =>
            _db.Barbers.AnyAsync(x => x.UserId == userId, ct);

        public async Task<IReadOnlyList<Barber>> ListByBarbershopAsync(Guid barbershopId, bool includeInactive, CancellationToken ct = default)
        {
            var q = _db.Barbers.AsNoTracking().Where(x => x.BarbershopId == barbershopId);
            if (!includeInactive)
                q = q.Where(x => x.IsActive);

            var rows = await q.ToListAsync(ct);
            return rows.Select(BarberMapper.ToDomain).ToList();
        }

        public async Task AddAsync(Barber barber, CancellationToken ct = default)
        {
            await _db.Barbers.AddAsync(BarberMapper.ToDbModel(barber), ct);
        }

        public void Update(Barber barber)
        {
            _db.Barbers.Update(BarberMapper.ToDbModel(barber));
        }
    }
}
