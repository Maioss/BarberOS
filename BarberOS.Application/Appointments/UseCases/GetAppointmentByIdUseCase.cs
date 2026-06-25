using BarberOS.Application.Appointments.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Appointments.UseCases
{
    public class GetAppointmentByIdUseCase
    {
        private readonly IAppointmentRepository _appointments;

        public GetAppointmentByIdUseCase(IAppointmentRepository appointments) =>
            _appointments = appointments;

        public async Task<AppointmentDto> ExecuteAsync(Guid id, CancellationToken ct = default)
        {
            var appointment = await _appointments.GetByIdAsync(id, ct)
                ?? throw NotFoundException.For("reserva", id);

            return CreateAppointmentUseCase.MapToDto(appointment);
        }
    }
}
