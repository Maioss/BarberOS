using BarberOS.Application.Metrics.DTOs;

namespace BarberOS.Application.Shared
{
    public interface IMetricsRepository
    {
        Task<BarbershopMetricsDto?> GetBarbershopMetricsAsync(Guid principalBarbershopId, DateOnly from, DateOnly to, CancellationToken ct = default);
        Task<BarberMetricsDto?> GetBarberMetricsAsync(Guid barberId, DateOnly from, DateOnly to, CancellationToken ct = default);
    }
}
