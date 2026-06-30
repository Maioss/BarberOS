using BarberOS.Application.Shared;
using BarberOS.Domain.Enums;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Appointments.UseCases
{
    public class CompleteAppointmentUseCase
    {
        private readonly IAppointmentRepository _appointments;
        private readonly IBarberRepository _barbers;
        private readonly IUnitOfWork _uow;

        public CompleteAppointmentUseCase(
            IAppointmentRepository appointments,
            IBarberRepository barbers,
            IUnitOfWork uow)
        {
            _appointments = appointments;
            _barbers = barbers;
            _uow = uow;
        }

        public async Task ExecuteAsync(Guid id, Guid requestingUserId, Role requestingRole, CancellationToken ct = default)
        {
            var appointment = await _appointments.GetByIdAsync(id, ct)
                ?? throw NotFoundException.For("reserva", id);

            if (requestingRole == Role.Barber)
            {
                var requestingBarber = await _barbers.GetByUserIdAsync(requestingUserId, ct)
                    ?? throw NotFoundException.For("barbero", requestingUserId);
                if (appointment.BarberId != requestingBarber.Id)
                    throw new ForbiddenException("No tienes permiso para completar esta reserva.");
            }

            appointment.Complete();

            var assignedBarber = await _barbers.GetByIdAsync(appointment.BarberId, ct)
                ?? throw NotFoundException.For("barbero", appointment.BarberId);

            assignedBarber.AddToBalance(appointment.TotalPrice);

            _appointments.Update(appointment);
            _barbers.Update(assignedBarber);
            await _uow.SaveChangesAsync(ct);
        }
    }
}
