using BarberOS.Application.Appointments.DTOs;
using BarberOS.Application.Shared;

namespace BarberOS.Application.Appointments.UseCases
{
    public class ListMyAppointmentsUseCase
    {
        private readonly IAppointmentRepository _appointments;
        private readonly IBarberRepository _barbers;
        private readonly IUserRepository _users;

        public ListMyAppointmentsUseCase(
            IAppointmentRepository appointments,
            IBarberRepository barbers,
            IUserRepository users)
        {
            _appointments = appointments;
            _barbers = barbers;
            _users = users;
        }

        public async Task<PagedResult<AppointmentDto>> ExecuteAsync(
            Guid clientId,
            AppointmentFilter filter,
            CancellationToken ct = default)
        {
            var result = await _appointments.ListByClientAsync(clientId, filter, ct);
            var dtos = new List<AppointmentDto>();
            foreach (var a in result.Items)
                dtos.Add(await CreateAppointmentUseCase.MapToDtoAsync(a, _users, _barbers, ct));
            return new PagedResult<AppointmentDto>(dtos, result.Page, result.PageSize, result.TotalCount);
        }
    }
}
