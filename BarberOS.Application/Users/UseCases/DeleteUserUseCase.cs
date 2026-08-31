using BarberOS.Application.Shared;
using BarberOS.Domain.Enums;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Users.UseCases
{
    public class DeleteUserUseCase
    {
        private readonly IUserRepository _users;
        private readonly ICurrentUserService _current;
        private readonly TenantScope _scope;
        private readonly IUnitOfWork _uow;

        public DeleteUserUseCase(
            IUserRepository users,
            ICurrentUserService current,
            TenantScope scope,
            IUnitOfWork uow)
        {
            _users = users;
            _current = current;
            _scope = scope;
            _uow = uow;
        }

        public async Task ExecuteAsync(Guid id, CancellationToken ct = default)
        {
            if (_current.UserId == id)
                throw new BusinessRuleException("No puedes desactivar tu propia cuenta.");

            var actorRole = _current.Role
                ?? throw new UnauthorizedException("No autenticado.");

            var user = await _users.GetByIdAsync(id, ct)
                ?? throw NotFoundException.For("usuario", id);

            if (actorRole != Role.SuperAdmin)
            {
                if (user.Role is Role.SuperAdmin or Role.Admin)
                    throw new ForbiddenException("No puedes desactivar cuentas de administrador.");

                if (user.BarbershopId is not null)
                    await _scope.EnsureInScopeAsync(user.BarbershopId.Value, ct);
            }

            user.Deactivate();
            _users.Update(user);
            await _uow.SaveChangesAsync(ct);
        }
    }
}
