using BarberOS.Application.Shared;
using BarberOS.Application.Users.DTOs;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Users.UseCases
{
    public class GetUserByIdUseCase
    {
        private readonly IUserRepository _users;

        public GetUserByIdUseCase(IUserRepository users) => _users = users;

        public async Task<UserDto> ExecuteAsync(Guid id, CancellationToken ct = default)
        {
            var u = await _users.GetByIdAsync(id, ct)
                ?? throw NotFoundException.For("usuario", id);

            return new UserDto(u.Id, u.Email, u.FullName, u.Phone, u.PhotoUrl, u.Role, u.BarbershopId, u.IsActive, u.CreatedAt);
        }
    }
}
