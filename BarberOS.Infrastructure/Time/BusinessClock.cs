using System.Collections.Concurrent;
using BarberOS.Application.Shared;
using BarberOS.Domain.Entities;

namespace BarberOS.Infrastructure.Time
{
    public class BusinessClock : IBusinessClock
    {
        private static readonly ConcurrentDictionary<string, TimeZoneInfo> Cache = new();

        public virtual DateTime UtcNow => DateTime.UtcNow;

        public DateOnly Today(Barbershop shop) => DateOnly.FromDateTime(LocalNow(shop));

        public TimeOnly TimeNow(Barbershop shop) => TimeOnly.FromDateTime(LocalNow(shop));

        private DateTime LocalNow(Barbershop shop) =>
            TimeZoneInfo.ConvertTimeFromUtc(UtcNow, Resolve(shop.TimeZoneId));

        private static TimeZoneInfo Resolve(string? timeZoneId) =>
            Cache.GetOrAdd(
                string.IsNullOrWhiteSpace(timeZoneId) ? Barbershop.DefaultTimeZoneId : timeZoneId,
                id =>
                {
                    try
                    {
                        return TimeZoneInfo.FindSystemTimeZoneById(id);
                    }
                    catch (Exception e) when (e is TimeZoneNotFoundException or InvalidTimeZoneException)
                    {
                        return TimeZoneInfo.FindSystemTimeZoneById(Barbershop.DefaultTimeZoneId);
                    }
                });
    }
}
