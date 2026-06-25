namespace BarberOS.Application.Appointments.DTOs
{
    public record AppointmentServiceDto(
        Guid ServiceId,
        string ServiceName,
        decimal Price,
        int DurationMinutes);
}
