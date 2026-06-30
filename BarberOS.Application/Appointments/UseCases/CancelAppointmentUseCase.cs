using BarberOS.Application.Shared;
using BarberOS.Domain.Enums;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Appointments.UseCases
{
    public class CancelAppointmentUseCase
    {
        private readonly IAppointmentRepository _appointments;
        private readonly IBarberRepository _barbers;
        private readonly IUnitOfWork _uow;

        public CancelAppointmentUseCase(IAppointmentRepository appointments, IBarberRepository barbers, IUnitOfWork uow)
        {
            _appointments = appointments;
            _barbers = barbers;
            _uow = uow;
        }

        public async Task ExecuteAsync(Guid id, Guid requestingUserId, Role requestingRole, CancellationToken ct = default)
        {
            var appointment = await _appointments.GetByIdAsync(id, ct)
                ?? throw NotFoundException.For("reserva", id);

            if (requestingRole == Role.Client && appointment.ClientId != requestingUserId)
                throw new ForbiddenException("No tienes permiso para cancelar esta reserva.");

            if (requestingRole == Role.Barber)
            {
                var barber = await _barbers.GetByUserIdAsync(requestingUserId, ct)
                    ?? throw NotFoundException.For("barbero", requestingUserId);
                if (appointment.BarberId != barber.Id)
                    throw new ForbiddenException("No tienes permiso para cancelar esta reserva.");
            }

            appointment.Cancel();
            _appointments.Update(appointment);
            await _uow.SaveChangesAsync(ct);
        }
    }
}
