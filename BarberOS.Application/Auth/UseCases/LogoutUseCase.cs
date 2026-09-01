using BarberOS.Application.Auth.DTOs;
using BarberOS.Application.Shared;

namespace BarberOS.Application.Auth.UseCases
{
    public class LogoutUseCase
    {
        private readonly IRefreshTokenRepository _refreshTokens;
        private readonly IRefreshTokenFactory _factory;
        private readonly IBusinessClock _clock;
        private readonly IUnitOfWork _uow;

        public LogoutUseCase(
            IRefreshTokenRepository refreshTokens,
            IRefreshTokenFactory factory,
            IBusinessClock clock,
            IUnitOfWork uow)
        {
            _refreshTokens = refreshTokens;
            _factory = factory;
            _clock = clock;
            _uow = uow;
        }

        /// <summary>Cerrar sesion con un token ya invalido no es un error: el efecto buscado ya se cumplio.</summary>
        public async Task ExecuteAsync(RefreshSessionRequest request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken)) return;

            var stored = await _refreshTokens.GetByHashAsync(_factory.Hash(request.RefreshToken), ct);
            if (stored is null || stored.IsRevoked) return;

            stored.Revoke(_clock.UtcNow);
            _refreshTokens.Update(stored);
            await _uow.SaveChangesAsync(ct);
        }
    }
}
