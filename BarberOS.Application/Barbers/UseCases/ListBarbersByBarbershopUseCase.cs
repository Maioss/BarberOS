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
            var shop = await _shops.GetByIdAsync(barbershopId, ct)
                ?? throw NotFoundException.For("barbería", barbershopId);

            // For a main barbershop, aggregate barbers from all its branches.
            // Barbers belong to specific locations (branches), not to the parent.
            var shopIds = new List<Guid> { barbershopId };
            if (shop.IsMain)
            {
                var branches = await _shops.ListBranchesAsync(barbershopId, ct);
                shopIds.AddRange(branches.Select(b => b.Id));
            }

            var allBarbers = new List<BarberDto>();
            foreach (var sid in shopIds)
            {
                var barbers = await _barbers.ListByBarbershopAsync(sid, includeInactive: false, ct);
                foreach (var b in barbers)
                {
                    var user = await _users.GetByIdAsync(b.UserId, ct);
                    if (user is null) continue;

                    allBarbers.Add(new BarberDto(
                        b.Id, user.Id, user.FullName, user.Phone, b.BarbershopId,
                        b.LunchStart, b.LunchEnd, b.GetAvailableDays(), b.IsActive
                    ));
                }
            }
            return allBarbers;
        }
    }
}
