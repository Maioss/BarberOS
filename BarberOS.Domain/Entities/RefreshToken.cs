using BarberOS.Domain.Exceptions;

namespace BarberOS.Domain.Entities
{
    /// <summary>
    /// Solo se guarda el hash: si alguien lee la tabla, no obtiene tokens usables.
    /// </summary>
    public class RefreshToken
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string TokenHash { get; private set; } = null!;
        public DateTime ExpiresAt { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? RevokedAt { get; private set; }
        public Guid? ReplacedByTokenId { get; private set; }

        private RefreshToken() { }

        public static RefreshToken Issue(Guid userId, string tokenHash, DateTime now, TimeSpan lifetime)
        {
            if (userId == Guid.Empty)
                throw new BusinessRuleException("El token debe pertenecer a un usuario.");

            if (string.IsNullOrWhiteSpace(tokenHash))
                throw new BusinessRuleException("El token debe tener un hash.");

            if (lifetime <= TimeSpan.Zero)
                throw new BusinessRuleException("La vigencia del token debe ser positiva.");

            return new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TokenHash = tokenHash,
                CreatedAt = now,
                ExpiresAt = now.Add(lifetime)
            };
        }

        public bool IsExpiredAt(DateTime now) => now >= ExpiresAt;

        public bool IsRevoked => RevokedAt is not null;

        public bool IsUsableAt(DateTime now) => !IsRevoked && !IsExpiredAt(now);

        public void Revoke(DateTime now)
        {
            if (IsRevoked) return;
            RevokedAt = now;
        }

        public void RotateInto(RefreshToken replacement, DateTime now)
        {
            if (!IsUsableAt(now))
                throw new UnauthorizedException("La sesión ya no es válida.");

            RevokedAt = now;
            ReplacedByTokenId = replacement.Id;
        }
    }
}
