using BarberOS.Application.Appointments.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Appointments.UseCases
{
    public class GetAppointmentByIdUseCase
    {
        private readonly IAppointmentRepository _appointments;
        private readonly IBarberRepository _barbers;
        private readonly IUserRepository _users;

        public GetAppointmentByIdUseCase(
            IAppointmentRepository appointments,
            IBarberRepository barbers,
            IUserRepository users)
        {
            _appointments = appointments;
            _barbers = barbers;
            _users = users;
        }

        public async Task<AppointmentDto> ExecuteAsync(Guid id, CancellationToken ct = default)
        {
            var appointment = await _appointments.GetByIdAsync(id, ct)
                ?? throw NotFoundException.For("reserva", id);

            return await CreateAppointmentUseCase.MapToDtoAsync(appointment, _users, _barbers, ct);
        }
    }
}
