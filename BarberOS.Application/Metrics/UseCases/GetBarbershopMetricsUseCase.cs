using BarberOS.Application.Metrics.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Metrics.UseCases
{
    public class GetBarbershopMetricsUseCase
    {
        private readonly IMetricsRepository _metrics;
        private readonly IBarbershopRepository _shops;

        public GetBarbershopMetricsUseCase(IMetricsRepository metrics, IBarbershopRepository shops)
        {
            _metrics = metrics;
            _shops = shops;
        }

        public async Task<BarbershopMetricsDto> ExecuteAsync(Guid barbershopId, MetricsQuery query, CancellationToken ct = default)
        {
            var shop = await _shops.GetByIdAsync(barbershopId, ct)
                ?? throw NotFoundException.For("barbería", barbershopId);

            var principalId = shop.IsMain ? shop.Id : shop.ParentId!.Value;

            var (from, to) = ResolvePeriod(query);

            return await _metrics.GetBarbershopMetricsAsync(principalId, from, to, ct)
                ?? throw NotFoundException.For("barbería principal", principalId);
        }

        private static (DateOnly From, DateOnly To) ResolvePeriod(MetricsQuery query)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var to = query.To ?? today;
            var from = query.From ?? to.AddDays(-30);

            if (from > to)
                throw new BusinessRuleException("La fecha inicial no puede ser posterior a la final.");

            if (to > today)
                throw new BusinessRuleException("Las métricas no se calculan para fechas futuras.");

            return (from, to);
        }
    }
}
