using BarberOS.Application.Auth.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Auth.UseCases
{

    public class LoginUseCase
    {
        private readonly IUserRepository _users;
        private readonly IPasswordHasher _hasher;
        private readonly IJwtTokenGenerator _jwt;

        public LoginUseCase(IUserRepository users, IPasswordHasher hasher, IJwtTokenGenerator jwt)
        {
            _users = users;
            _hasher = hasher;
            _jwt = jwt;
        }

        public async Task<AuthResponse> ExecuteAsync(LoginRequest request, CancellationToken ct = default)
        {
            var user = await _users.GetByEmailAsync(request.Email, ct);

            if (user is null || !user.IsActive)
                throw new UnauthorizedException("Credenciales inválidas.");

            if (!_hasher.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedException("Credenciales inválidas.");

            var token = _jwt.Generate(user);
            var info = new UserInfo(user.Id, user.Email, user.FullName, user.Role, user.BarbershopId, user.PhotoUrl);
            return new AuthResponse(token, info);
        }
    }
}
