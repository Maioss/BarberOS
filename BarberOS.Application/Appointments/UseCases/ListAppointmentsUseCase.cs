using BarberOS.Application.Appointments.DTOs;
using BarberOS.Application.Shared;

namespace BarberOS.Application.Appointments.UseCases
{
    public class ListAppointmentsUseCase
    {
        private readonly IAppointmentRepository _appointments;
        private readonly IBarberRepository _barbers;
        private readonly IUserRepository _users;
        private readonly TenantScope _scope;

        public ListAppointmentsUseCase(
            IAppointmentRepository appointments,
            IBarberRepository barbers,
            IUserRepository users,
            TenantScope scope)
        {
            _appointments = appointments;
            _barbers = barbers;
            _users = users;
            _scope = scope;
        }

        public async Task<PagedResult<AppointmentDto>> ExecuteAsync(
            AppointmentFilter filter,
            CancellationToken ct = default)
        {
            var allowed = await _scope.VisibleSiteIdsAsync(ct);
            var sites = allowed;

            if (filter.BarbershopId is not null)
            {
                await _scope.EnsureInScopeAsync(filter.BarbershopId.Value, ct);
                var requested = await _scope.SitesCoveredByAsync(filter.BarbershopId.Value, ct);
                sites = allowed is null ? requested : requested.Where(allowed.Contains).ToList();
            }

            var scoped = filter with { BarbershopId = null, BarbershopIds = sites };

            var result = await _appointments.ListAsync(scoped, ct);
            var dtos = await CreateAppointmentUseCase.MapManyAsync(result.Items, _users, _barbers, ct);
            return new PagedResult<AppointmentDto>(dtos, result.Page, result.PageSize, result.TotalCount);
        }
    }
}
