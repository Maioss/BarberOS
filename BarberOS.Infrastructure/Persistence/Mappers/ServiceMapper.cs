using System.Reflection;
using BarberOS.Domain.Entities;
using BarberOS.Infrastructure.Persistence.DbModels;

namespace BarberOS.Infrastructure.Persistence.Mappers
{
    public static class ServiceMapper
    {
        public static ServiceDbModel ToDbModel(Service s) => new()
        {
            Id = s.Id,
            BarbershopId = s.BarbershopId,
            Name = s.Name,
            Description = s.Description,
            Price = s.Price,
            DurationMinutes = s.DurationMinutes,
            IsActive = s.IsActive,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        };

        public static Service ToDomain(ServiceDbModel db)
        {
            var entity = (Service)Activator.CreateInstance(typeof(Service), nonPublic: true)!;
            var t = typeof(Service);
            SetPrivate(entity, t, nameof(Service.Id), db.Id);
            SetPrivate(entity, t, nameof(Service.BarbershopId), db.BarbershopId);
            SetPrivate(entity, t, nameof(Service.Name), db.Name);
            SetPrivate(entity, t, nameof(Service.Description), db.Description);
            SetPrivate(entity, t, nameof(Service.Price), db.Price);
            SetPrivate(entity, t, nameof(Service.DurationMinutes), db.DurationMinutes);
            SetPrivate(entity, t, nameof(Service.IsActive), db.IsActive);
            SetPrivate(entity, t, nameof(Service.CreatedAt), db.CreatedAt);
            SetPrivate(entity, t, nameof(Service.UpdatedAt), db.UpdatedAt);
            return entity;
        }

        private static void SetPrivate(object instance, Type type, string propertyName, object? value)
        {
            var prop = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)!;
            prop.GetSetMethod(nonPublic: true)!.Invoke(instance, [value]);
        }
    }
}
