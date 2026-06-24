using BarberOS.Application.Auth.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Auth.UseCases
{

    public class GetCurrentUserUseCase
    {
        private readonly ICurrentUserService _current;
        private readonly IUserRepository _users;

        public GetCurrentUserUseCase(ICurrentUserService current, IUserRepository users)
        {
            _current = current;
            _users = users;
        }

        public async Task<UserInfo> ExecuteAsync(CancellationToken ct = default)
        {
            if (!_current.IsAuthenticated || _current.UserId is null)
                throw new UnauthorizedException("No autenticado.");

            var user = await _users.GetByIdAsync(_current.UserId.Value, ct)
                ?? throw new NotFoundException("Usuario no encontrado.");

            return new UserInfo(user.Id, user.Email, user.FullName, user.Role, user.BarbershopId);
        }
    }
}
