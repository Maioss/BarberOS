using BarberOS.Domain.Entities;
using BarberOS.Infrastructure.Time;

namespace BarberOS.Api.IntegrationTests;

/// <summary>
/// Desplaza el reloj para que las pruebas no dependan de la hora a la que se corran:
/// completar una cita exige que sea de hoy, y despues de las 18:00 no quedan huecos.
/// </summary>
public class ShiftableClock : BusinessClock
{
    private TimeSpan _offset = TimeSpan.Zero;

    public override DateTime UtcNow => DateTime.UtcNow + _offset;

    /// <summary>Deja la hora local de la sede en el proximo dia laboral a <paramref name="time"/>.</summary>
    public void MoveToWorkingDay(TimeOnly time)
    {
        var zone = TimeZoneInfo.FindSystemTimeZoneById(Barbershop.DefaultTimeZoneId);

        var day = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zone));
        while (day.DayOfWeek == DayOfWeek.Sunday) day = day.AddDays(1);

        _offset = TimeZoneInfo.ConvertTimeToUtc(day.ToDateTime(time), zone) - DateTime.UtcNow;
    }

    public void Reset() => _offset = TimeSpan.Zero;
}
