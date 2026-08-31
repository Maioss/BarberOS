using BarberOS.Domain.Enums;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Shared
{
    /// <summary>
    /// Resuelve a qué barbería pertenece quien hace la petición y verifica que no
    /// salga de ahí. Los barberos y las citas cuelgan de sucursales, mientras que un
    /// Admin está asociado a la sede principal, así que toda comparación se hace
    /// entre sedes principales.
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

        /// <summary>Un SuperAdmin ve todas las barberías.</summary>
        public bool IsUnrestricted => _current.Role == Role.SuperAdmin;

        /// <summary>Sede principal a la que pertenece una barbería (ella misma si ya lo es).</summary>
        public async Task<Guid> PrincipalIdAsync(Guid barbershopId, CancellationToken ct = default)
        {
            var shop = await _shops.GetByIdAsync(barbershopId, ct)
                ?? throw NotFoundException.For("barbería", barbershopId);

            return shop.IsMain ? shop.Id : shop.ParentId!.Value;
        }

        /// <summary>
        /// Sede principal del usuario actual, o <c>null</c> cuando no tiene restricción.
        /// </summary>
        public async Task<Guid?> ActorPrincipalIdAsync(CancellationToken ct = default)
        {
            if (IsUnrestricted) return null;

            var own = _current.BarbershopId
                ?? throw new ForbiddenException("Tu usuario no está asociado a ninguna barbería.");

            return await PrincipalIdAsync(own, ct);
        }

        /// <summary>Falla si el usuario actual no puede tocar esa barbería.</summary>
        public async Task EnsureInScopeAsync(Guid barbershopId, CancellationToken ct = default)
        {
            var actor = await ActorPrincipalIdAsync(ct);
            if (actor is null) return;

            var target = await PrincipalIdAsync(barbershopId, ct);
            if (target != actor)
                throw new ForbiddenException("No tienes acceso a los datos de esa barbería.");
        }

        /// <summary>Sede principal más todas sus sucursales.</summary>
        public async Task<IReadOnlyList<Guid>> SiteIdsAsync(Guid principalId, CancellationToken ct = default)
        {
            var branches = await _shops.ListBranchesAsync(principalId, ct);

            var ids = new List<Guid>(branches.Count + 1) { principalId };
            ids.AddRange(branches.Select(b => b.Id));
            return ids;
        }

        /// <summary>
        /// Sedes que cubre una barbería: ella sola si es sucursal, o ella y todas sus
        /// sucursales si es principal. Las citas y los pagos cuelgan de sucursales, así
        /// que filtrar por la principal a secas no devuelve nada.
        /// </summary>
        public async Task<IReadOnlyList<Guid>> SitesCoveredByAsync(Guid barbershopId, CancellationToken ct = default)
        {
            var shop = await _shops.GetByIdAsync(barbershopId, ct)
                ?? throw NotFoundException.For("barbería", barbershopId);

            return shop.IsMain
                ? await SiteIdsAsync(shop.Id, ct)
                : new[] { shop.Id };
        }

        /// <summary>
        /// Sedes que el usuario actual puede consultar, o <c>null</c> cuando no tiene
        /// restricción y por tanto no hay que filtrar.
        /// </summary>
        public async Task<IReadOnlyList<Guid>?> VisibleSiteIdsAsync(CancellationToken ct = default)
        {
            var principal = await ActorPrincipalIdAsync(ct);
            return principal is null ? null : await SiteIdsAsync(principal.Value, ct);
        }
    }
}
