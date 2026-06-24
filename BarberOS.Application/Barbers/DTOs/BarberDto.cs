namespace BarberOS.Application.Barbers.DTOs
{
    public record BarberDto(
        Guid Id,
        Guid UserId,
        string FullName,
        string? Phone,
        Guid BarbershopId,
        TimeOnly LunchStart,
        TimeOnly LunchEnd,
        IReadOnlyList<DayOfWeek> AvailableDays,
        bool IsActive
    );
}
