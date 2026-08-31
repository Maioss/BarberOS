using BarberOS.Application.Shared;
using BarberOS.Application.Users.DTOs;
using BarberOS.Domain.Enums;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Users.UseCases
{
    public class UpdateUserUseCase
    {
        private readonly IUserRepository _users;
        private readonly IBarbershopRepository _shops;
        private readonly ICurrentUserService _current;
        private readonly TenantScope _scope;
        private readonly IUnitOfWork _uow;

        public UpdateUserUseCase(
            IUserRepository users,
            IBarbershopRepository shops,
            ICurrentUserService current,
            TenantScope scope,
            IUnitOfWork uow)
        {
            _users = users;
            _shops = shops;
            _current = current;
            _scope = scope;
            _uow = uow;
        }

        public async Task<UserDto> ExecuteAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default)
        {
            var user = await _users.GetByIdAsync(id, ct)
                ?? throw NotFoundException.For("usuario", id);

            var actorRole = _current.Role
                ?? throw new UnauthorizedException("No autenticado.");

            var isSelf = _current.UserId == user.Id;
            var actorIsSuperAdmin = actorRole == Role.SuperAdmin;

            if (isSelf && !actorIsSuperAdmin && user.Role != request.Role)
                throw new ForbiddenException("No puedes cambiar tu propio rol.");

            if (!actorIsSuperAdmin && !isSelf && user.Role is Role.SuperAdmin or Role.Admin)
                throw new ForbiddenException("No puedes modificar cuentas de administrador.");

            if (!actorIsSuperAdmin && request.Role != user.Role && request.Role is Role.SuperAdmin or Role.Admin)
                throw new ForbiddenException("Solo un SuperAdmin puede otorgar roles de administrador.");

            if (!isSelf && user.BarbershopId is not null)
                await _scope.EnsureInScopeAsync(user.BarbershopId.Value, ct);

            if (request.BarbershopId is not null)
            {
                var shop = await _shops.GetByIdAsync(request.BarbershopId.Value, ct)
                    ?? throw NotFoundException.For("barbería", request.BarbershopId.Value);

                if (!shop.IsActive)
                    throw new BusinessRuleException("No se puede asociar un usuario a una barbería inactiva.");

                await _scope.EnsureInScopeAsync(shop.Id, ct);
            }

            user.UpdateProfile(request.FullName, request.Phone);
            user.ChangeRole(request.Role, request.BarbershopId);

            _users.Update(user);
            await _uow.SaveChangesAsync(ct);

            return new UserDto(user.Id, user.Email, user.FullName, user.Phone, user.PhotoUrl, user.Role, user.BarbershopId, user.IsActive, user.CreatedAt);
        }
    }
}
