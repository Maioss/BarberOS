using BarberOS.Application.Shared;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Users.UseCases
{
    public class DeleteUserUseCase
    {
        private readonly IUserRepository _users;
        private readonly ICurrentUserService _current;
        private readonly IUnitOfWork _uow;

        public DeleteUserUseCase(IUserRepository users, ICurrentUserService current, IUnitOfWork uow)
        {
            _users = users;
            _current = current;
            _uow = uow;
        }

        public async Task ExecuteAsync(Guid id, CancellationToken ct = default)
        {
            if (_current.UserId == id)
                throw new BusinessRuleException("No puedes desactivar tu propia cuenta.");

            var user = await _users.GetByIdAsync(id, ct)
                ?? throw NotFoundException.For("usuario", id);

            user.Deactivate();
            _users.Update(user);
            await _uow.SaveChangesAsync(ct);
        }
    }
}
