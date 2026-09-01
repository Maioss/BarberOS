using BarberOS.Domain.Entities;

namespace BarberOS.Domain.Tests;

internal static class TestData
{
    public static readonly DateOnly Today = new(2026, 9, 2);

    public static Service NewService(string name = "Corte", decimal price = 25000m, int minutes = 30) =>
        Service.Create(Guid.NewGuid(), name, null, price, minutes);

    public static Appointment NewAppointment(
        DateOnly? date = null,
        TimeOnly? start = null,
        params Service[] services) =>
        Appointment.Create(
            clientId: Guid.NewGuid(),
            barberId: Guid.NewGuid(),
            barbershopId: Guid.NewGuid(),
            date: date ?? Today,
            startTime: start ?? new TimeOnly(10, 0),
            services: services.Length > 0 ? services : [NewService()]);

    public static Barber NewBarber() => Barber.Create(Guid.NewGuid(), Guid.NewGuid());
}
