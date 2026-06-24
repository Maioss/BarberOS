using BarberOS.Domain.Entities;
using BarberOS.Infrastructure.Persistence.DbModels;
using System.Reflection;

namespace BarberOS.Infrastructure.Persistence.Mappers
{
    public static class BarbershopMapper
    {
        public static BarbershopDbModel ToDbModel(Barbershop b)
        {
            return new BarbershopDbModel
            {
                Id = b.Id,
                Name = b.Name,
                Address = b.Address,
                City = b.City,
                Phone = b.Phone,
                IsMain = b.IsMain,
                ParentId = b.ParentId,
                IsActive = b.IsActive,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            };
        }

        public static Barbershop ToDomain(BarbershopDbModel row)
        {
            var b = (Barbershop)Activator.CreateInstance(typeof(Barbershop), nonPublic: true)!;
            Set(b, nameof(Barbershop.Id), row.Id);
            Set(b, nameof(Barbershop.Name), row.Name);
            Set(b, nameof(Barbershop.Address), row.Address);
            Set(b, nameof(Barbershop.City), row.City);
            Set(b, nameof(Barbershop.Phone), row.Phone);
            Set(b, nameof(Barbershop.IsMain), row.IsMain);
            Set(b, nameof(Barbershop.ParentId), row.ParentId);
            Set(b, nameof(Barbershop.IsActive), row.IsActive);
            Set(b, nameof(Barbershop.CreatedAt), row.CreatedAt);
            Set(b, nameof(Barbershop.UpdatedAt), row.UpdatedAt);
            return b;
        }

        private static void Set(object target, string propertyName, object? value)
        {
            var prop = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance)!;
            prop.GetSetMethod(nonPublic: true)!.Invoke(target, [value]);
        }
    }
}
