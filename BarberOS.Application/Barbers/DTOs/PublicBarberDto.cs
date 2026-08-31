namespace BarberOS.Application.Barbers.DTOs
{
    /// <summary>
    /// Barbero tal como lo ve un visitante anonimo en la landing y en el flujo de
    /// reserva: sin datos de contacto ni el id de usuario, que solo hacen falta en
    /// el panel de administracion.
    /// </summary>
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
