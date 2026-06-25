using BarberOS.Application.Metrics.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Enums;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Metrics.UseCases
{
    public class GetMyBarberMetricsUseCase
    {
        private readonly IMetricsRepository _metrics;
        private readonly IBarberRepository _barbers;
        private readonly ICurrentUserService _current;

        public GetMyBarberMetricsUseCase(IMetricsRepository metrics, IBarberRepository barbers, ICurrentUserService current)
        {
            _metrics = metrics;
            _barbers = barbers;
            _current = current;
        }

        public async Task<BarberMetricsDto> ExecuteAsync(MetricsQuery query, CancellationToken ct = default)
        {
            if (!_current.IsAuthenticated || _current.UserId is null)
                throw new UnauthorizedException("No autenticado.");

            if (_current.Role != Role.Barber)
                throw new ForbiddenException("Solo barberos pueden consultar sus métricas.");

            var barber = await _barbers.GetByUserIdAsync(_current.UserId.Value, ct)
                ?? throw new NotFoundException("No tienes un perfil de barbero registrado.");

            var (from, to) = ResolvePeriod(query);

            return await _metrics.GetBarberMetricsAsync(barber.Id, from, to, ct)
                ?? throw NotFoundException.For("barbero", barber.Id);
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
