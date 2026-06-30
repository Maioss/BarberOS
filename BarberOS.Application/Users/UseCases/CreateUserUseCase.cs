using BarberOS.Application.Shared;
using BarberOS.Application.Users.DTOs;
using BarberOS.Domain.Entities;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Users.UseCases
{
    public class CreateUserUseCase
    {
        private readonly IUserRepository _users;
        private readonly IBarbershopRepository _shops;
        private readonly IPasswordHasher _hasher;
        private readonly IUnitOfWork _uow;

        public CreateUserUseCase(IUserRepository users, IBarbershopRepository shops, IPasswordHasher hasher, IUnitOfWork uow)
        {
            _users = users;
            _shops = shops;
            _hasher = hasher;
            _uow = uow;
        }

        public async Task<UserDto> ExecuteAsync(CreateUserRequest request, CancellationToken ct = default)
        {
            if (await _users.ExistsByEmailAsync(request.Email, ct))
                throw new ConflictException("Ya existe una cuenta con ese correo.");

            if (request.BarbershopId is not null)
            {
                var shop = await _shops.GetByIdAsync(request.BarbershopId.Value, ct)
                    ?? throw NotFoundException.For("barbería", request.BarbershopId.Value);

                if (!shop.IsActive)
                    throw new BusinessRuleException("No se puede asociar un usuario a una barbería inactiva.");
            }

            var hash = _hasher.Hash(request.Password);
            var user = User.Create(request.Email, hash, request.FullName, request.Role, request.Phone, request.BarbershopId);

            await _users.AddAsync(user, ct);
            await _uow.SaveChangesAsync(ct);

            return new UserDto(user.Id, user.Email, user.FullName, user.Phone, user.PhotoUrl, user.Role, user.BarbershopId, user.IsActive, user.CreatedAt);
        }
    }
}
