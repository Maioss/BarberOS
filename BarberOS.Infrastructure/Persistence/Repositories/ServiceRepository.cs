using BarberOS.Application.Shared;
using BarberOS.Domain.Entities;
using BarberOS.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace BarberOS.Infrastructure.Persistence.Repositories
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly BarberOSDbContext _db;

        public ServiceRepository(BarberOSDbContext db) => _db = db;

        public async Task<Service?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var row = await _db.Services.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
            return row is null ? null : ServiceMapper.ToDomain(row);
        }

        public async Task<IReadOnlyList<Service>> ListByBarbershopAsync(Guid barbershopId, bool includeInactive, CancellationToken ct = default)
        {
            var q = _db.Services.AsNoTracking().Where(x => x.BarbershopId == barbershopId);
            if (!includeInactive)
                q = q.Where(x => x.IsActive);

            var rows = await q.OrderBy(x => x.Name).ToListAsync(ct);
            return rows.Select(ServiceMapper.ToDomain).ToList();
        }

        public async Task<IReadOnlyList<Service>> GetManyByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        {
            var idList = ids.Distinct().ToList();
            if (idList.Count == 0) return Array.Empty<Service>();

            var rows = await _db.Services.AsNoTracking()
                .Where(x => idList.Contains(x.Id))
                .ToListAsync(ct);

            return rows.Select(ServiceMapper.ToDomain).ToList();
        }

        public async Task AddAsync(Service service, CancellationToken ct = default)
        {
            await _db.Services.AddAsync(ServiceMapper.ToDbModel(service), ct);
        }

        public void Update(Service service)
        {
            _db.Services.Update(ServiceMapper.ToDbModel(service));
        }
    }
}
