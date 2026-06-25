using System.Reflection;
using BarberOS.Domain.Entities;
using BarberOS.Domain.Enums;
using BarberOS.Infrastructure.Persistence.DbModels;

namespace BarberOS.Infrastructure.Persistence.Mappers
{
    public static class PaymentMapper
    {
        public static PaymentDbModel ToDbModel(Payment p) => new()
        {
            Id = p.Id,
            AppointmentId = p.AppointmentId,
            ClientId = p.ClientId,
            BarberId = p.BarberId,
            BarbershopId = p.BarbershopId,
            Amount = p.Amount,
            Method = (int)p.Method,
            Status = (int)p.Status,
            Notes = p.Notes,
            PaidAt = p.PaidAt,
            RefundedAt = p.RefundedAt,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        };

        public static Payment ToDomain(PaymentDbModel db)
        {
            var entity = (Payment)Activator.CreateInstance(typeof(Payment), nonPublic: true)!;
            var t = typeof(Payment);

            SetPrivate(entity, t, nameof(Payment.Id), db.Id);
            SetPrivate(entity, t, nameof(Payment.AppointmentId), db.AppointmentId);
            SetPrivate(entity, t, nameof(Payment.ClientId), db.ClientId);
            SetPrivate(entity, t, nameof(Payment.BarberId), db.BarberId);
            SetPrivate(entity, t, nameof(Payment.BarbershopId), db.BarbershopId);
            SetPrivate(entity, t, nameof(Payment.Amount), db.Amount);
            SetPrivate(entity, t, nameof(Payment.Method), (PaymentMethod)db.Method);
            SetPrivate(entity, t, nameof(Payment.Status), (PaymentStatus)db.Status);
            SetPrivate(entity, t, nameof(Payment.Notes), db.Notes);
            SetPrivate(entity, t, nameof(Payment.PaidAt), db.PaidAt);
            SetPrivate(entity, t, nameof(Payment.RefundedAt), db.RefundedAt);
            SetPrivate(entity, t, nameof(Payment.CreatedAt), db.CreatedAt);
            SetPrivate(entity, t, nameof(Payment.UpdatedAt), db.UpdatedAt);

            return entity;
        }

        private static void SetPrivate(object instance, Type type, string propertyName, object? value)
        {
            var prop = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)!;
            prop.GetSetMethod(nonPublic: true)!.Invoke(instance, [value]);
        }
    }
}
