using BarberOS.Application.Shared;
using BarberOS.Application.Users.DTOs;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Users.UseCases
{
    /// <summary>
    /// Perfil del usuario autenticado. Existe para que el frontend no tenga que pedir
    /// su propio perfil al endpoint generico de usuarios, que es de administracion.
    /// </summary>
    public class GetMyProfileUseCase
    {
        private readonly IUserRepository _users;
        private readonly ICurrentUserService _current;

        public GetMyProfileUseCase(IUserRepository users, ICurrentUserService current)
        {
            _users = users;
            _current = current;
        }

        public async Task<UserDto> ExecuteAsync(CancellationToken ct = default)
        {
            if (!_current.IsAuthenticated || _current.UserId is null)
                throw new UnauthorizedException("No autenticado.");

            var u = await _users.GetByIdAsync(_current.UserId.Value, ct)
                ?? throw new NotFoundException("Usuario no encontrado.");

            return new UserDto(u.Id, u.Email, u.FullName, u.Phone, u.PhotoUrl, u.Role, u.BarbershopId, u.IsActive, u.CreatedAt);
        }
    }
}
