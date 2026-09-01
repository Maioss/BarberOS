using BarberOS.Application.Auth.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Entities;

namespace BarberOS.Application.Auth
{
    public class RefreshTokenOptions
    {
        public int RefreshExpirationDays { get; set; } = 14;
    }

    public class SessionIssuer
    {
        private readonly IJwtTokenGenerator _jwt;
        private readonly IRefreshTokenRepository _refreshTokens;
        private readonly IRefreshTokenFactory _factory;
        private readonly IBusinessClock _clock;
        private readonly RefreshTokenOptions _options;

        public SessionIssuer(
            IJwtTokenGenerator jwt,
            IRefreshTokenRepository refreshTokens,
            IRefreshTokenFactory factory,
            IBusinessClock clock,
            RefreshTokenOptions options)
        {
            _jwt = jwt;
            _refreshTokens = refreshTokens;
            _factory = factory;
            _clock = clock;
            _options = options;
        }

        public async Task<AuthResponse> IssueAsync(User user, CancellationToken ct = default)
        {
            var (token, hash) = _factory.Create();

            var refreshToken = RefreshToken.Issue(
                user.Id, hash, _clock.UtcNow, TimeSpan.FromDays(_options.RefreshExpirationDays));

            await _refreshTokens.AddAsync(refreshToken, ct);

            return Build(user, token);
        }

        public async Task<AuthResponse> RotateAsync(User user, RefreshToken current, CancellationToken ct = default)
        {
            var (token, hash) = _factory.Create();

            var replacement = RefreshToken.Issue(
                user.Id, hash, _clock.UtcNow, TimeSpan.FromDays(_options.RefreshExpirationDays));

            current.RotateInto(replacement, _clock.UtcNow);

            await _refreshTokens.AddAsync(replacement, ct);
            _refreshTokens.Update(current);

            return Build(user, token);
        }

        private AuthResponse Build(User user, string refreshToken) => new(
            _jwt.Generate(user),
            refreshToken,
            new UserInfo(user.Id, user.Email, user.FullName, user.Role, user.BarbershopId, user.PhotoUrl));
    }
}
