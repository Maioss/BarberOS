using BarberOS.Domain.Enums;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Shared
{
    /// <summary>
    /// Un Admin esta asociado a la sede principal, mientras que barberos, citas y pagos
    /// cuelgan de sucursales: toda comparacion se hace entre sedes principales.
    /// </summary>
    public class TenantScope
    {
        private readonly ICurrentUserService _current;
        private readonly IBarbershopRepository _shops;

        public TenantScope(ICurrentUserService current, IBarbershopRepository shops)
        {
            _current = current;
            _shops = shops;
        }

        public bool IsUnrestricted => _current.Role == Role.SuperAdmin;

        public async Task<Guid> PrincipalIdAsync(Guid barbershopId, CancellationToken ct = default)
        {
            var shop = await _shops.GetByIdAsync(barbershopId, ct)
                ?? throw NotFoundException.For("barbería", barbershopId);

            return shop.IsMain ? shop.Id : shop.ParentId!.Value;
        }

        /// <summary><c>null</c> cuando el usuario no tiene restriccion de sede.</summary>
        public async Task<Guid?> ActorPrincipalIdAsync(CancellationToken ct = default)
        {
            if (IsUnrestricted) return null;

            var own = _current.BarbershopId
                ?? throw new ForbiddenException("Tu usuario no está asociado a ninguna barbería.");

            return await PrincipalIdAsync(own, ct);
        }

        public async Task EnsureInScopeAsync(Guid barbershopId, CancellationToken ct = default)
        {
            var actor = await ActorPrincipalIdAsync(ct);
            if (actor is null) return;

            var target = await PrincipalIdAsync(barbershopId, ct);
            if (target != actor)
                throw new ForbiddenException("No tienes acceso a los datos de esa barbería.");
        }

        public async Task<IReadOnlyList<Guid>> SiteIdsAsync(Guid principalId, CancellationToken ct = default)
        {
            var branches = await _shops.ListBranchesAsync(principalId, ct);

            var ids = new List<Guid>(branches.Count + 1) { principalId };
            ids.AddRange(branches.Select(b => b.Id));
            return ids;
        }

        public async Task<IReadOnlyList<Guid>> SitesCoveredByAsync(Guid barbershopId, CancellationToken ct = default)
        {
            var shop = await _shops.GetByIdAsync(barbershopId, ct)
                ?? throw NotFoundException.For("barbería", barbershopId);

            return shop.IsMain
                ? await SiteIdsAsync(shop.Id, ct)
                : new[] { shop.Id };
        }

        /// <summary><c>null</c> cuando no hay que filtrar.</summary>
        public async Task<IReadOnlyList<Guid>?> VisibleSiteIdsAsync(CancellationToken ct = default)
        {
            var principal = await ActorPrincipalIdAsync(ct);
            return principal is null ? null : await SiteIdsAsync(principal.Value, ct);
        }
    }
}
