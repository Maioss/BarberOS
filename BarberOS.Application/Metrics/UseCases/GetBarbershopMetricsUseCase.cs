using BarberOS.Application.Metrics.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Metrics.UseCases
{
    public class GetBarbershopMetricsUseCase
    {
        private readonly IMetricsRepository _metrics;
        private readonly IBarbershopRepository _shops;
        private readonly IBusinessClock _clock;
        private readonly TenantScope _scope;

        public GetBarbershopMetricsUseCase(
            IMetricsRepository metrics,
            IBarbershopRepository shops,
            IBusinessClock clock,
            TenantScope scope)
        {
            _metrics = metrics;
            _shops = shops;
            _clock = clock;
            _scope = scope;
        }

        public async Task<BarbershopMetricsDto> ExecuteAsync(Guid barbershopId, MetricsQuery query, CancellationToken ct = default)
        {
            var shop = await _shops.GetByIdAsync(barbershopId, ct)
                ?? throw NotFoundException.For("barbería", barbershopId);

            await _scope.EnsureInScopeAsync(shop.Id, ct);

            var principalId = shop.IsMain ? shop.Id : shop.ParentId!.Value;

            var (from, to) = ResolvePeriod(query, _clock.Today(shop));

            return await _metrics.GetBarbershopMetricsAsync(principalId, from, to, ct)
                ?? throw NotFoundException.For("barbería principal", principalId);
        }

        private static (DateOnly From, DateOnly To) ResolvePeriod(MetricsQuery query, DateOnly today)
        {

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
