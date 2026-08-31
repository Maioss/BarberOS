using System.Reflection;
using BarberOS.Domain.Entities;
using BarberOS.Domain.Enums;
using BarberOS.Infrastructure.Persistence.DbModels;

namespace BarberOS.Infrastructure.Persistence.Mappers
{
    public static class BalanceEntryMapper
    {
        public static BalanceEntryDbModel ToDbModel(BalanceEntry e) => new()
        {
            Id = e.Id,
            BarberId = e.BarberId,
            Amount = e.Amount,
            Reason = (int)e.Reason,
            AppointmentId = e.AppointmentId,
            PaymentId = e.PaymentId,
            CreatedAt = e.CreatedAt
        };

        public static BalanceEntry ToDomain(BalanceEntryDbModel db)
        {
            var entity = (BalanceEntry)Activator.CreateInstance(typeof(BalanceEntry), nonPublic: true)!;
            var t = typeof(BalanceEntry);
            SetPrivate(entity, t, nameof(BalanceEntry.Id), db.Id);
            SetPrivate(entity, t, nameof(BalanceEntry.BarberId), db.BarberId);
            SetPrivate(entity, t, nameof(BalanceEntry.Amount), db.Amount);
            SetPrivate(entity, t, nameof(BalanceEntry.Reason), (BalanceEntryReason)db.Reason);
            SetPrivate(entity, t, nameof(BalanceEntry.AppointmentId), db.AppointmentId);
            SetPrivate(entity, t, nameof(BalanceEntry.PaymentId), db.PaymentId);
            SetPrivate(entity, t, nameof(BalanceEntry.CreatedAt), db.CreatedAt);
            return entity;
        }

        private static void SetPrivate(object instance, Type type, string propertyName, object? value)
        {
            var prop = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)!;
            prop.GetSetMethod(nonPublic: true)!.Invoke(instance, [value]);
        }
    }
}
