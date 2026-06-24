using BarberOS.Application.Barbers.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Barbers.UseCases
{
    public class GetBarberByIdUseCase
    {
        private readonly IBarberRepository _barbers;
        private readonly IUserRepository _users;

        public GetBarberByIdUseCase(IBarberRepository barbers, IUserRepository users)
        {
            _barbers = barbers;
            _users = users;
        }

        public async Task<BarberDto> ExecuteAsync(Guid id, CancellationToken ct = default)
        {
            var b = await _barbers.GetByIdAsync(id, ct)
                ?? throw NotFoundException.For("barbero", id);

            var user = await _users.GetByIdAsync(b.UserId, ct)
                ?? throw NotFoundException.For("usuario del barbero", b.UserId);

            return new BarberDto(
                b.Id, user.Id, user.FullName, user.Phone, b.BarbershopId,
                b.LunchStart, b.LunchEnd, b.GetAvailableDays(), b.IsActive
            );
        }
    }
}
