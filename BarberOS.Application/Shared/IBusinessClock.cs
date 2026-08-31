using BarberOS.Domain.Entities;

namespace BarberOS.Application.Shared
{
    /// <summary>
    /// Las citas se guardan en hora local de la barberia, no en UTC: "hoy" y "ahora"
    /// dependen de la sede.
    /// </summary>
    public interface IBusinessClock
    {
        DateTime UtcNow { get; }

        DateOnly Today(Barbershop shop);

        TimeOnly TimeNow(Barbershop shop);
    }
}
