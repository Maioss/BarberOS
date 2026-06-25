using BarberOS.Application.Shared;
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

        public async Task ExecuteAsync(Guid id, CancellationToken ct = default)
        {
            var appointment = await _appointments.GetByIdAsync(id, ct)
                ?? throw NotFoundException.For("reserva", id);

            appointment.Complete();

            var barber = await _barbers.GetByIdAsync(appointment.BarberId, ct)
                ?? throw NotFoundException.For("barbero", appointment.BarberId);

            barber.AddToBalance(appointment.TotalPrice);

            _appointments.Update(appointment);
            _barbers.Update(barber);
            await _uow.SaveChangesAsync(ct);
        }
    }
}
