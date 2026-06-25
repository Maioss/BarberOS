namespace BarberOS.Application.Metrics.DTOs
{
    public record TopServiceDto(Guid ServiceId, string Name, int Count, decimal Revenue);
}
