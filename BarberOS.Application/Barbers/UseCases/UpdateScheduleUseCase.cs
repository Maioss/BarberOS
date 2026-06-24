using BarberOS.Application.Barbers.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Enums;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Barbers.UseCases
{
    public class UpdateScheduleUseCase
    {
        private readonly IBarberRepository _barbers;
        private readonly IUserRepository _users;
        private readonly ICurrentUserService _current;
        private readonly IUnitOfWork _uow;

        public UpdateScheduleUseCase(IBarberRepository barbers, IUserRepository users, ICurrentUserService current, IUnitOfWork uow)
        {
            _barbers = barbers;
            _users = users;
            _current = current;
            _uow = uow;
        }

        public async Task<BarberDto> ExecuteAsync(Guid barberId, UpdateScheduleRequest request, CancellationToken ct = default)
        {
            var barber = await _barbers.GetByIdAsync(barberId, ct)
                ?? throw NotFoundException.For("barbero", barberId);

            var isOwner = _current.UserId == barber.UserId;
            var isAdmin = _current.Role == Role.SuperAdmin || _current.Role == Role.Admin;

            if (!isOwner && !isAdmin)
                throw new ForbiddenException("No tienes permiso para modificar este horario.");

            barber.UpdateSchedule(request.LunchStart, request.LunchEnd, request.AvailableDays);
            _barbers.Update(barber);
            await _uow.SaveChangesAsync(ct);

            var user = await _users.GetByIdAsync(barber.UserId, ct)
                ?? throw NotFoundException.For("usuario del barbero", barber.UserId);

            return new BarberDto(
                barber.Id, user.Id, user.FullName, user.Phone, barber.BarbershopId,
                barber.LunchStart, barber.LunchEnd, barber.GetAvailableDays(), barber.IsActive
            );
        }
    }
}
