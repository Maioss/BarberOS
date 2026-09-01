using BarberOS.Application.Shared;
using BarberOS.Domain.Entities;
using BarberOS.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace BarberOS.Infrastructure.Persistence.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly BarberOSDbContext _db;

        public RefreshTokenRepository(BarberOSDbContext db) => _db = db;

        public async Task AddAsync(RefreshToken token, CancellationToken ct = default)
        {
            await _db.RefreshTokens.AddAsync(RefreshTokenMapper.ToDbModel(token), ct);
        }

        public async Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default)
        {
            var row = await _db.RefreshTokens.AsNoTracking()
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);
            return row is null ? null : RefreshTokenMapper.ToDomain(row);
        }

        public async Task<IReadOnlyList<RefreshToken>> ListActiveByUserAsync(Guid userId, CancellationToken ct = default)
        {
            var rows = await _db.RefreshTokens.AsNoTracking()
                .Where(t => t.UserId == userId && t.RevokedAt == null)
                .ToListAsync(ct);
            return rows.Select(RefreshTokenMapper.ToDomain).ToList();
        }

        public void Update(RefreshToken token)
        {
            var tracked = _db.RefreshTokens.Local.FirstOrDefault(t => t.Id == token.Id);
            if (tracked is not null)
            {
                _db.Entry(tracked).CurrentValues.SetValues(RefreshTokenMapper.ToDbModel(token));
                return;
            }

            _db.Entry(RefreshTokenMapper.ToDbModel(token)).State = EntityState.Modified;
        }
    }
}
