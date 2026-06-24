using BarberOS.Domain.Enums;

namespace BarberOS.Application.Users.DTOs
{
    public record UpdateUserRequest(
        string FullName,
        string? Phone,
        Role Role,
        Guid? BarbershopId
    );
}
