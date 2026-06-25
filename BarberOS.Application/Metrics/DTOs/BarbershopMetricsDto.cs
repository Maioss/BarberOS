namespace BarberOS.Application.Metrics.DTOs
{
    public record BarbershopMetricsDto(
        Guid BarbershopId,
        string BarbershopName,
        DateOnly From,
        DateOnly To,
        int TotalAppointments,
        int CompletedAppointments,
        int CancelledAppointments,
        decimal CompletionRate,
        decimal GrossRevenue,
        decimal Refunds,
        decimal NetRevenue,
        IReadOnlyDictionary<string, decimal> PaymentsByMethod,
        IReadOnlyList<TopServiceDto> TopServices,
        IReadOnlyList<TopBarberDto> TopBarbers);
}
