using System.Reflection;
using BarberOS.Domain.Entities;
using BarberOS.Domain.Enums;
using BarberOS.Infrastructure.Persistence.DbModels;

namespace BarberOS.Infrastructure.Persistence.Mappers
{

    public static class UserMapper
    {
        public static UserDbModel ToDbModel(User user) => new()
        {
            Id = user.Id,
            Email = user.Email,
            PasswordHash = user.PasswordHash,
            FullName = user.FullName,
            Phone = user.Phone,
            PhotoUrl = user.PhotoUrl,
            Role = (int)user.Role,
            BarbershopId = user.BarbershopId,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };

        public static User ToDomain(UserDbModel db)
        {
            var user = (User)Activator.CreateInstance(typeof(User), nonPublic: true)!;
            var t = typeof(User);
            SetPrivate(user, t, nameof(User.Id), db.Id);
            SetPrivate(user, t, nameof(User.Email), db.Email);
            SetPrivate(user, t, nameof(User.PasswordHash), db.PasswordHash);
            SetPrivate(user, t, nameof(User.FullName), db.FullName);
            SetPrivate(user, t, nameof(User.Phone), db.Phone);
            SetPrivate(user, t, nameof(User.PhotoUrl), db.PhotoUrl);
            SetPrivate(user, t, nameof(User.Role), (Role)db.Role);
            SetPrivate(user, t, nameof(User.BarbershopId), db.BarbershopId);
            SetPrivate(user, t, nameof(User.IsActive), db.IsActive);
            SetPrivate(user, t, nameof(User.CreatedAt), db.CreatedAt);
            SetPrivate(user, t, nameof(User.UpdatedAt), db.UpdatedAt);
            return user;
        }

        private static void SetPrivate(object instance, Type type, string propertyName, object? value)
        {
            var prop = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)!;
            prop.GetSetMethod(nonPublic: true)!.Invoke(instance, new[] { value });
        }
    }
}
