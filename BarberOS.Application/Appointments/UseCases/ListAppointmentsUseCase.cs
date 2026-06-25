using BarberOS.Application.Appointments.DTOs;
using BarberOS.Application.Shared;

namespace BarberOS.Application.Appointments.UseCases
{
    public class ListAppointmentsUseCase
    {
        private readonly IAppointmentRepository _appointments;

        public ListAppointmentsUseCase(IAppointmentRepository appointments) =>
            _appointments = appointments;

        public async Task<PagedResult<AppointmentDto>> ExecuteAsync(
            AppointmentFilter filter,
            CancellationToken ct = default)
        {
            var result = await _appointments.ListAsync(filter, ct);
            return new PagedResult<AppointmentDto>(
                result.Items.Select(CreateAppointmentUseCase.MapToDto).ToList(),
                result.Page,
                result.PageSize,
                result.TotalCount);
        }
    }
}
