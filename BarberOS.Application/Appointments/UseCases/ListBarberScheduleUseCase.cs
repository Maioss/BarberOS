using BarberOS.Application.Appointments.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Enums;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Appointments.UseCases
{
    public class ListBarberScheduleUseCase
    {
        private readonly IAppointmentRepository _appointments;
        private readonly IBarberRepository _barbers;
        private readonly IUserRepository _users;
        private readonly ICurrentUserService _current;

        public ListBarberScheduleUseCase(
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

        public async Task<IReadOnlyList<AppointmentDto>> ExecuteAsync(CancellationToken ct = default)
        {
            if (!_current.IsAuthenticated || _current.UserId is null)
                throw new UnauthorizedException("No autenticado.");

            if (_current.Role != Role.Barber)
                throw new ForbiddenException("Solo barberos pueden consultar su agenda.");

            var barber = await _barbers.GetByUserIdAsync(_current.UserId.Value, ct)
                ?? throw new NotFoundException("No tienes un perfil de barbero registrado.");

            var appointments = await _appointments.ListByBarberAsync(barber.Id, ct);
            return await CreateAppointmentUseCase.MapManyAsync(appointments, _users, _barbers, ct);
        }
    }
}
