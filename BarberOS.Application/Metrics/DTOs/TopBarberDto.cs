namespace BarberOS.Application.Metrics.DTOs
{
    public record TopBarberDto(Guid BarberId, string Name, int CompletedAppointments, decimal Revenue);
}
