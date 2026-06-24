namespace BarberOS.Application.Barbers.DTOs
{
    public record UpdateScheduleRequest(
        TimeOnly LunchStart,
        TimeOnly LunchEnd,
        IReadOnlyList<DayOfWeek> AvailableDays
    );
}
