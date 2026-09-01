using BarberOS.Application.Auth.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Auth.UseCases
{

    public class LoginUseCase
    {
        private readonly IUserRepository _users;
        private readonly IPasswordHasher _hasher;
        private readonly SessionIssuer _issuer;
        private readonly IUnitOfWork _uow;

        public LoginUseCase(
            IUserRepository users,
            IPasswordHasher hasher,
            SessionIssuer issuer,
            IUnitOfWork uow)
        {
            _users = users;
            _hasher = hasher;
            _issuer = issuer;
            _uow = uow;
        }

        public async Task<AuthResponse> ExecuteAsync(LoginRequest request, CancellationToken ct = default)
        {
            var user = await _users.GetByEmailAsync(request.Email, ct);

            if (user is null || !user.IsActive)
                throw new UnauthorizedException("Credenciales inválidas.");

            if (!_hasher.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedException("Credenciales inválidas.");

            var response = await _issuer.IssueAsync(user, ct);
            await _uow.SaveChangesAsync(ct);
            return response;
        }
    }
}
