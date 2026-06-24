using BarberOS.Application.Shared;
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

        public async Task AddAsync(User user, CancellationToken ct = default)
        {
            var row = UserMapper.ToDbModel(user);
            await _db.Users.AddAsync(row, ct);
        }
    }
}
