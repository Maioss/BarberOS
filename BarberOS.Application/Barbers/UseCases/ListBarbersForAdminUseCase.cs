using BarberOS.Application.Barbers.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Barbers.UseCases
{
    /// <summary>
    /// Listado de barberos para el panel: incluye el id de usuario y el telefono,
    /// que el listado publico no expone. Acotado a la barberia del administrador.
    /// </summary>
    public class ListBarbersForAdminUseCase
    {
        private readonly IBarberRepository _barbers;
        private readonly IBarbershopRepository _shops;
        private readonly IUserRepository _users;
        private readonly TenantScope _scope;

        public ListBarbersForAdminUseCase(
            IBarberRepository barbers,
            IBarbershopRepository shops,
            IUserRepository users,
            TenantScope scope)
        {
            _barbers = barbers;
            _shops = shops;
            _users = users;
            _scope = scope;
        }

        public async Task<IReadOnlyList<BarberDto>> ExecuteAsync(Guid barbershopId, CancellationToken ct = default)
        {
            var shop = await _shops.GetByIdAsync(barbershopId, ct)
                ?? throw NotFoundException.For("barbería", barbershopId);

            await _scope.EnsureInScopeAsync(shop.Id, ct);

            var shopIds = new List<Guid> { shop.Id };
            if (shop.IsMain)
            {
                var branches = await _shops.ListBranchesAsync(shop.Id, ct);
                shopIds.AddRange(branches.Select(b => b.Id));
            }

            var result = new List<BarberDto>();
            foreach (var sid in shopIds)
            {
                var barbers = await _barbers.ListByBarbershopAsync(sid, includeInactive: false, ct);
                foreach (var b in barbers)
                {
                    var user = await _users.GetByIdAsync(b.UserId, ct);
                    if (user is null) continue;

                    result.Add(new BarberDto(
                        b.Id, user.Id, user.FullName, user.Phone, b.BarbershopId,
                        b.LunchStart, b.LunchEnd, b.GetAvailableDays(), b.IsActive
                    ));
                }
            }
            return result;
        }
    }
}
