using BarberOS.Application.Shared;
using BarberOS.Domain.Entities;
using BarberOS.Domain.Enums;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Appointments.UseCases
{
    public class CompleteAppointmentUseCase
    {
        private readonly IAppointmentRepository _appointments;
        private readonly IBarberRepository _barbers;
        private readonly IBarbershopRepository _shops;
        private readonly IBalanceEntryRepository _ledger;
        private readonly IBusinessClock _clock;
        private readonly TenantScope _scope;
        private readonly ICurrentUserService _current;
        private readonly IUnitOfWork _uow;

        public CompleteAppointmentUseCase(
            IAppointmentRepository appointments,
            IBarberRepository barbers,
            IBarbershopRepository shops,
            IBalanceEntryRepository ledger,
            IBusinessClock clock,
            TenantScope scope,
            ICurrentUserService current,
            IUnitOfWork uow)
        {
            _appointments = appointments;
            _barbers = barbers;
            _shops = shops;
            _ledger = ledger;
            _clock = clock;
            _scope = scope;
            _current = current;
            _uow = uow;
        }

        public async Task ExecuteAsync(Guid id, CancellationToken ct = default)
        {
            var requestingUserId = _current.RequireUserId();
            var requestingRole = _current.RequireRole();

            var appointment = await _appointments.GetByIdAsync(id, ct)
                ?? throw NotFoundException.For("reserva", id);

            if (requestingRole == Role.Barber)
            {
                var requestingBarber = await _barbers.GetByUserIdAsync(requestingUserId, ct)
                    ?? throw NotFoundException.For("barbero", requestingUserId);
                if (appointment.BarberId != requestingBarber.Id)
                    throw new ForbiddenException("No tienes permiso para completar esta reserva.");
            }

            if (requestingRole is Role.Admin or Role.SuperAdmin)
                await _scope.EnsureInScopeAsync(appointment.BarbershopId, ct);

            var shop = await _shops.GetByIdAsync(appointment.BarbershopId, ct)
                ?? throw NotFoundException.For("barbería", appointment.BarbershopId);

            appointment.Complete(_clock.Today(shop));

            var assignedBarber = await _barbers.GetByIdAsync(appointment.BarberId, ct)
                ?? throw NotFoundException.For("barbero", appointment.BarberId);

            var credit = BalanceEntry.ForCompletedAppointment(
                assignedBarber.Id, appointment.Id, appointment.TotalPrice);

            _appointments.Update(appointment);
            await _ledger.AddAsync(credit, ct);
            await _uow.SaveChangesAsync(ct);
        }
    }
}
