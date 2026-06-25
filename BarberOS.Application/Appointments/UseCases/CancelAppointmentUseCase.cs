using BarberOS.Application.Shared;
using BarberOS.Domain.Enums;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Appointments.UseCases
{
    public class CancelAppointmentUseCase
    {
        private readonly IAppointmentRepository _appointments;
        private readonly IUnitOfWork _uow;

        public CancelAppointmentUseCase(IAppointmentRepository appointments, IUnitOfWork uow)
        {
            _appointments = appointments;
            _uow = uow;
        }

        public async Task ExecuteAsync(Guid id, Guid requestingUserId, Role requestingRole, CancellationToken ct = default)
        {
            var appointment = await _appointments.GetByIdAsync(id, ct)
                ?? throw NotFoundException.For("reserva", id);

            if (requestingRole == Role.Client && appointment.ClientId != requestingUserId)
                throw new ForbiddenException("No tienes permiso para cancelar esta reserva.");

            appointment.Cancel();
            _appointments.Update(appointment);
            await _uow.SaveChangesAsync(ct);
        }
    }
}
