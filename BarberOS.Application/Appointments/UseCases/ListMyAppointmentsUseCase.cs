using BarberOS.Application.Appointments.DTOs;
using BarberOS.Application.Shared;

namespace BarberOS.Application.Appointments.UseCases
{
    public class ListMyAppointmentsUseCase
    {
        private readonly IAppointmentRepository _appointments;

        public ListMyAppointmentsUseCase(IAppointmentRepository appointments) =>
            _appointments = appointments;

        public async Task<PagedResult<AppointmentDto>> ExecuteAsync(
            Guid clientId,
            AppointmentFilter filter,
            CancellationToken ct = default)
        {
            var result = await _appointments.ListByClientAsync(clientId, filter, ct);
            return new PagedResult<AppointmentDto>(
                result.Items.Select(CreateAppointmentUseCase.MapToDto).ToList(),
                result.Page,
                result.PageSize,
                result.TotalCount);
        }
    }
}
