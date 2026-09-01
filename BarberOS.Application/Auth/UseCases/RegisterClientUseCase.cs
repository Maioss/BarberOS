using BarberOS.Application.Auth.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Entities;
using BarberOS.Domain.Enums;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Auth.UseCases
{

    public class RegisterClientUseCase
    {
        private readonly IUserRepository _users;
        private readonly IPasswordHasher _hasher;
        private readonly SessionIssuer _issuer;
        private readonly IUnitOfWork _uow;

        public RegisterClientUseCase(
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

        public async Task<AuthResponse> ExecuteAsync(RegisterClientRequest request, CancellationToken ct = default)
        {
            if (await _users.ExistsByEmailAsync(request.Email, ct))
                throw new ConflictException("Ya existe una cuenta con ese correo.");

            var hash = _hasher.Hash(request.Password);
            var user = User.Create(request.Email, hash, request.FullName, Role.Client, request.Phone);

            await _users.AddAsync(user, ct);

            var response = await _issuer.IssueAsync(user, ct);
            await _uow.SaveChangesAsync(ct);
            return response;
        }
    }
}
