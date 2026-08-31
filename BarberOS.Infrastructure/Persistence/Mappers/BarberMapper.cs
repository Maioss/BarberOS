using System.Reflection;
using BarberOS.Domain.Entities;
using BarberOS.Infrastructure.Persistence.DbModels;

namespace BarberOS.Infrastructure.Persistence.Mappers
{
    public static class BarberMapper
    {
        public static BarberDbModel ToDbModel(Barber b) => new()
        {
            Id = b.Id,
            UserId = b.UserId,
            BarbershopId = b.BarbershopId,
            LunchStart = b.LunchStart,
            LunchEnd = b.LunchEnd,
            AvailableDays = b.AvailableDays,
            IsActive = b.IsActive,
            CreatedAt = b.CreatedAt,
            UpdatedAt = b.UpdatedAt
        };

        public static Barber ToDomain(BarberDbModel db)
        {
            var entity = (Barber)Activator.CreateInstance(typeof(Barber), nonPublic: true)!;
            var t = typeof(Barber);
            SetPrivate(entity, t, nameof(Barber.Id), db.Id);
            SetPrivate(entity, t, nameof(Barber.UserId), db.UserId);
            SetPrivate(entity, t, nameof(Barber.BarbershopId), db.BarbershopId);
            SetPrivate(entity, t, nameof(Barber.LunchStart), db.LunchStart);
            SetPrivate(entity, t, nameof(Barber.LunchEnd), db.LunchEnd);
            SetPrivate(entity, t, nameof(Barber.AvailableDays), db.AvailableDays);
            SetPrivate(entity, t, nameof(Barber.IsActive), db.IsActive);
            SetPrivate(entity, t, nameof(Barber.CreatedAt), db.CreatedAt);
            SetPrivate(entity, t, nameof(Barber.UpdatedAt), db.UpdatedAt);
            return entity;
        }

        private static void SetPrivate(object instance, Type type, string propertyName, object? value)
        {
            var prop = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)!;
            prop.GetSetMethod(nonPublic: true)!.Invoke(instance, [value]);
        }
    }
}
