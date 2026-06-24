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
        private readonly IJwtTokenGenerator _jwt;
        private readonly IUnitOfWork _uow;

        public RegisterClientUseCase(IUserRepository users, IPasswordHasher hasher, IJwtTokenGenerator jwt, IUnitOfWork uow)
        {
            _users = users;
            _hasher = hasher;
            _jwt = jwt;
            _uow = uow;
        }

        public async Task<AuthResponse> ExecuteAsync(RegisterClientRequest request, CancellationToken ct = default)
        {
            var emailExists = await _users.ExistsByEmailAsync(request.Email, ct);
            if (emailExists)
                throw new ConflictException("Ya existe una cuenta con ese correo.");

            var hash = _hasher.Hash(request.Password);
            var user = User.Create(request.Email, hash, request.FullName, Role.Client, request.Phone);

            await _users.AddAsync(user, ct);
            await _uow.SaveChangesAsync(ct);

            var token = _jwt.Generate(user);
            var info = new UserInfo(user.Id, user.Email, user.FullName, user.Role, user.BarbershopId);
            return new AuthResponse(token, info);
        }
    }
}
