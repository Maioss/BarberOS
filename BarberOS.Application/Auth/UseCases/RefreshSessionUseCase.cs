using BarberOS.Application.Auth.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Auth.UseCases
{
    public class RefreshSessionUseCase
    {
        private readonly IRefreshTokenRepository _refreshTokens;
        private readonly IRefreshTokenFactory _factory;
        private readonly IUserRepository _users;
        private readonly SessionIssuer _issuer;
        private readonly IBusinessClock _clock;
        private readonly IUnitOfWork _uow;

        public RefreshSessionUseCase(
            IRefreshTokenRepository refreshTokens,
            IRefreshTokenFactory factory,
            IUserRepository users,
            SessionIssuer issuer,
            IBusinessClock clock,
            IUnitOfWork uow)
        {
            _refreshTokens = refreshTokens;
            _factory = factory;
            _users = users;
            _issuer = issuer;
            _clock = clock;
            _uow = uow;
        }

        public async Task<AuthResponse> ExecuteAsync(RefreshSessionRequest request, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
                throw new UnauthorizedException("Sesión inválida.");

            var stored = await _refreshTokens.GetByHashAsync(_factory.Hash(request.RefreshToken), ct)
                ?? throw new UnauthorizedException("Sesión inválida.");

            // Presentar un token ya rotado significa que alguien tiene una copia:
            // se cierran todas las sesiones de ese usuario.
            if (stored.IsRevoked)
            {
                await RevokeEverythingFor(stored.UserId, ct);
                await _uow.SaveChangesAsync(ct);
                throw new UnauthorizedException("Sesión inválida.");
            }

            if (stored.IsExpiredAt(_clock.UtcNow))
                throw new UnauthorizedException("La sesión expiró.");

            var user = await _users.GetByIdAsync(stored.UserId, ct);
            if (user is null || !user.IsActive)
            {
                stored.Revoke(_clock.UtcNow);
                _refreshTokens.Update(stored);
                await _uow.SaveChangesAsync(ct);
                throw new UnauthorizedException("Sesión inválida.");
            }

            var response = await _issuer.RotateAsync(user, stored, ct);
            await _uow.SaveChangesAsync(ct);
            return response;
        }

        private async Task RevokeEverythingFor(Guid userId, CancellationToken ct)
        {
            foreach (var token in await _refreshTokens.ListActiveByUserAsync(userId, ct))
            {
                token.Revoke(_clock.UtcNow);
                _refreshTokens.Update(token);
            }
        }
    }
}
