using BarberOS.Domain.Enums;

namespace BarberOS.Application.Users.DTOs
{
    public record CreateUserRequest(
        string Email,
        string Password,
        string FullName,
        string? Phone,
        Role Role,
        Guid? BarbershopId
    );
}
