namespace BarberOS.Application.Barbers.DTOs
{
    public record PublicBarberDto(
        Guid Id,
        string FullName,
        Guid BarbershopId,
        TimeOnly LunchStart,
        TimeOnly LunchEnd,
        IReadOnlyList<DayOfWeek> AvailableDays,
        bool IsActive
    );
}
