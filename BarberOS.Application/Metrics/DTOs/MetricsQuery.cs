namespace BarberOS.Application.Metrics.DTOs
{
    public record MetricsQuery(DateOnly? From = null, DateOnly? To = null);
}
