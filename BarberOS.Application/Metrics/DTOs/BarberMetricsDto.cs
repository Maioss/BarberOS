namespace BarberOS.Application.Metrics.DTOs
{
    public record BarberMetricsDto(
        Guid BarberId,
        string BarberName,
        Guid BarbershopId,
        DateOnly From,
        DateOnly To,
        int TotalAppointments,
        int CompletedAppointments,
        int CancelledAppointments,
        decimal CompletionRate,
        decimal GrossRevenue,
        decimal CurrentBalance,
        IReadOnlyList<TopServiceDto> TopServices);
}
