namespace BarberOS.Application.Appointments.DTOs
{
    public record CreateAppointmentRequest(
        Guid BarberId,
        DateOnly Date,
        TimeOnly StartTime,
        List<Guid> ServiceIds,
        string? Notes,
        Guid? ClientId = null);
}
