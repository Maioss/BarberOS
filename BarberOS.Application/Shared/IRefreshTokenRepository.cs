using BarberOS.Domain.Entities;

namespace BarberOS.Application.Shared
{
    public interface IRefreshTokenRepository
    {
        Task AddAsync(RefreshToken token, CancellationToken ct = default);
        Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken ct = default);
        Task<IReadOnlyList<RefreshToken>> ListActiveByUserAsync(Guid userId, CancellationToken ct = default);
        void Update(RefreshToken token);
    }
}
