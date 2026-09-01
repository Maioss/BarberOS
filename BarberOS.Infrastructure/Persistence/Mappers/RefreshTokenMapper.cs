using System.Reflection;
using BarberOS.Domain.Entities;
using BarberOS.Infrastructure.Persistence.DbModels;

namespace BarberOS.Infrastructure.Persistence.Mappers
{
    public static class RefreshTokenMapper
    {
        public static RefreshTokenDbModel ToDbModel(RefreshToken t) => new()
        {
            Id = t.Id,
            UserId = t.UserId,
            TokenHash = t.TokenHash,
            ExpiresAt = t.ExpiresAt,
            CreatedAt = t.CreatedAt,
            RevokedAt = t.RevokedAt,
            ReplacedByTokenId = t.ReplacedByTokenId
        };

        public static RefreshToken ToDomain(RefreshTokenDbModel db)
        {
            var entity = (RefreshToken)Activator.CreateInstance(typeof(RefreshToken), nonPublic: true)!;
            var t = typeof(RefreshToken);
            Set(entity, t, nameof(RefreshToken.Id), db.Id);
            Set(entity, t, nameof(RefreshToken.UserId), db.UserId);
            Set(entity, t, nameof(RefreshToken.TokenHash), db.TokenHash);
            Set(entity, t, nameof(RefreshToken.ExpiresAt), db.ExpiresAt);
            Set(entity, t, nameof(RefreshToken.CreatedAt), db.CreatedAt);
            Set(entity, t, nameof(RefreshToken.RevokedAt), db.RevokedAt);
            Set(entity, t, nameof(RefreshToken.ReplacedByTokenId), db.ReplacedByTokenId);
            return entity;
        }

        private static void Set(object instance, Type type, string propertyName, object? value)
        {
            var prop = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)!;
            prop.GetSetMethod(nonPublic: true)!.Invoke(instance, [value]);
        }
    }
}
