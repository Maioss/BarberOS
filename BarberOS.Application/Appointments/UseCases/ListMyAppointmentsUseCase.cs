using BarberOS.Application.Appointments.DTOs;
using BarberOS.Application.Shared;

namespace BarberOS.Application.Appointments.UseCases
{
    public class ListMyAppointmentsUseCase
    {
        private readonly IAppointmentRepository _appointments;
        private readonly IBarberRepository _barbers;
        private readonly IUserRepository _users;
        private readonly ICurrentUserService _current;

        public ListMyAppointmentsUseCase(
            IAppointmentRepository appointments,
            IBarberRepository barbers,
            IUserRepository users,
            ICurrentUserService current)
        {
            _appointments = appointments;
            _barbers = barbers;
            _users = users;
            _current = current;
        }

        public async Task<PagedResult<AppointmentDto>> ExecuteAsync(
            AppointmentFilter filter,
            CancellationToken ct = default)
        {
            var result = await _appointments.ListByClientAsync(_current.RequireUserId(), filter, ct);
            var dtos = await CreateAppointmentUseCase.MapManyAsync(result.Items, _users, _barbers, ct);
            return new PagedResult<AppointmentDto>(dtos, result.Page, result.PageSize, result.TotalCount);
        }
    }
}
