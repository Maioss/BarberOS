using BarberOS.Domain.Enums;

namespace BarberOS.Application.Users.DTOs
{
    public record UserDto(
        Guid Id,
        string Email,
        string FullName,
        string? Phone,
        string? PhotoUrl,
        Role Role,
        Guid? BarbershopId,
        bool IsActive,
        DateTime CreatedAt
    );
}
