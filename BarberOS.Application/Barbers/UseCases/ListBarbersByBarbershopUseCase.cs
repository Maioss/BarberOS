using BarberOS.Application.Barbers.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Barbers.UseCases
{
    public class ListBarbersByBarbershopUseCase
    {
        private readonly IBarberRepository _barbers;
        private readonly IBarbershopRepository _shops;
        private readonly IUserRepository _users;

        public ListBarbersByBarbershopUseCase(IBarberRepository barbers, IBarbershopRepository shops, IUserRepository users)
        {
            _barbers = barbers;
            _shops = shops;
            _users = users;
        }

        public async Task<IReadOnlyList<BarberDto>> ExecuteAsync(Guid barbershopId, CancellationToken ct = default)
        {
            _ = await _shops.GetByIdAsync(barbershopId, ct)
                ?? throw NotFoundException.For("barbería", barbershopId);

            var barbers = await _barbers.ListByBarbershopAsync(barbershopId, includeInactive: false, ct);

            var result = new List<BarberDto>();
            foreach (var b in barbers)
            {
                var user = await _users.GetByIdAsync(b.UserId, ct);
                if (user is null) continue;

                result.Add(new BarberDto(
                    b.Id, user.Id, user.FullName, user.Phone, b.BarbershopId,
                    b.LunchStart, b.LunchEnd, b.GetAvailableDays(), b.IsActive
                ));
            }
            return result;
        }
    }
}
