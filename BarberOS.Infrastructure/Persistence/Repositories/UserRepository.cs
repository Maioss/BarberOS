using BarberOS.Application.Shared;
using BarberOS.Application.Users.DTOs;
using BarberOS.Domain.Entities;
using BarberOS.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace BarberOS.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly BarberOSDbContext _db;

        public UserRepository(BarberOSDbContext db) => _db = db;

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var row = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);
            return row is null ? null : UserMapper.ToDomain(row);
        }

        public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            var normalized = email.Trim().ToLowerInvariant();
            var row = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == normalized, ct);
            return row is null ? null : UserMapper.ToDomain(row);
        }

        public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        {
            var normalized = email.Trim().ToLowerInvariant();
            return _db.Users.AnyAsync(u => u.Email == normalized, ct);
        }

        public async Task<PagedResult<User>> ListAsync(UserFilter filter, CancellationToken ct = default)
        {
            var q = _db.Users.AsNoTracking().AsQueryable();

            if (filter.Role.HasValue)
                q = q.Where(x => x.Role == (int)filter.Role.Value);

            if (filter.BarbershopId.HasValue)
                q = q.Where(x => x.BarbershopId == filter.BarbershopId.Value);

            if (filter.IsActive.HasValue)
                q = q.Where(x => x.IsActive == filter.IsActive.Value);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.Trim().ToLower();
                q = q.Where(x => x.Email.Contains(search) || x.FullName.ToLower().Contains(search));
            }

            var total = await q.CountAsync(ct);

            var rows = await q.OrderBy(x => x.FullName)
                .Skip(filter.Skip)
                .Take(filter.Take)
                .ToListAsync(ct);

            var items = rows.Select(UserMapper.ToDomain).ToList();
            return new PagedResult<User>(items, filter.Page, filter.Take, total);
        }

        public async Task AddAsync(User user, CancellationToken ct = default)
        {
            var row = UserMapper.ToDbModel(user);
            await _db.Users.AddAsync(row, ct);
        }

        public void Update(User user)
        {
            _db.Users.Update(UserMapper.ToDbModel(user));
        }
    }
}
