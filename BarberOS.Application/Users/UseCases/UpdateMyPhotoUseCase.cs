using BarberOS.Application.Shared;
using BarberOS.Application.Users.DTOs;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Users.UseCases;

public class UpdateMyPhotoUseCase
{
    private readonly IUserRepository _users;
    private readonly ICurrentUserService _current;
    private readonly IUnitOfWork _uow;

    public UpdateMyPhotoUseCase(IUserRepository users, ICurrentUserService current, IUnitOfWork uow)
    {
        _users = users;
        _current = current;
        _uow = uow;
    }

    public async Task<UserDto> ExecuteAsync(UpdateMyPhotoRequest request, CancellationToken ct = default)
    {
        if (!_current.IsAuthenticated || _current.UserId is null)
            throw new UnauthorizedException("No autenticado.");

        if (request.PhotoUrl is not null && request.PhotoUrl.Length > 2048)
            throw new BusinessRuleException("La URL de la foto es demasiado larga.");

        var user = await _users.GetByIdAsync(_current.UserId.Value, ct)
            ?? throw new NotFoundException("Usuario no encontrado.");

        user.UpdatePhoto(request.PhotoUrl);
        _users.Update(user);
        await _uow.SaveChangesAsync(ct);

        return new UserDto(user.Id, user.Email, user.FullName, user.Phone, user.PhotoUrl, user.Role, user.BarbershopId, user.IsActive, user.CreatedAt);
    }
}
