namespace BarberOS.Application.Barbers.DTOs
{
    /// <summary>Sin datos de contacto ni id de usuario: lo consume la landing sin autenticar.</summary>
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
