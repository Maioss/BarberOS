using BarberOS.Application.Barbershops.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Entities;
using BarberOS.Infrastructure.Persistence.DbModels;
using BarberOS.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace BarberOS.Infrastructure.Persistence.Repositories
{
    public class BarbershopRepository : IBarbershopRepository
    {
        private readonly BarberOSDbContext _db;

        public BarbershopRepository(BarberOSDbContext db)
        {
            _db = db;
        }

        public async Task<Barbershop?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var row = await _db.Barbershops.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id, ct);
            return row is null ? null : BarbershopMapper.ToDomain(row);
        }

        public async Task<PagedResult<Barbershop>> ListAsync(BarbershopFilter filter, CancellationToken ct = default)
        {
            var query = _db.Barbershops.AsNoTracking().AsQueryable();

            if (filter.City is not null)
                query = query.Where(b => b.City.ToLower().Contains(filter.City.ToLower()));

            if (filter.IsMain.HasValue)
                query = query.Where(b => b.IsMain == filter.IsMain.Value);

            if (filter.IsActive.HasValue)
                query = query.Where(b => b.IsActive == filter.IsActive.Value);

            var total = await query.CountAsync(ct);
            var rows = await query
                .OrderBy(b => b.City).ThenBy(b => b.Name)
                .Skip(filter.Skip).Take(filter.Take)
                .ToListAsync(ct);

            var items = rows.Select(BarbershopMapper.ToDomain).ToList();
            return new PagedResult<Barbershop>(items, filter.Page, filter.Take, total);
        }

        public async Task<IReadOnlyList<Barbershop>> ListBranchesAsync(Guid parentId, CancellationToken ct = default)
        {
            var rows = await _db.Barbershops.AsNoTracking()
                .Where(b => b.ParentId == parentId)
                .OrderBy(b => b.Name)
                .ToListAsync(ct);
            return rows.Select(BarbershopMapper.ToDomain).ToList();
        }

        public async Task<bool> HasActiveBranchesAsync(Guid parentId, CancellationToken ct = default)
        {
            return await _db.Barbershops.AnyAsync(b => b.ParentId == parentId && b.IsActive, ct);
        }

        public async Task AddAsync(Barbershop barbershop, CancellationToken ct = default)
        {
            await _db.Barbershops.AddAsync(BarbershopMapper.ToDbModel(barbershop), ct);
        }

        public void Update(Barbershop barbershop)
        {
            _db.Barbershops.Update(BarbershopMapper.ToDbModel(barbershop));
        }

        public void Remove(Barbershop barbershop)
        {
            var row = BarbershopMapper.ToDbModel(barbershop);
            _db.Barbershops.Attach(row);
            _db.Barbershops.Remove(row);
        }
    }
}
