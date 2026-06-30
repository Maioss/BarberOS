using BarberOS.Domain.Enums;

namespace BarberOS.Application.Auth.DTOs
{

    public record AuthResponse(string Token, UserInfo User);

    public record UserInfo(Guid Id, string Email, string FullName, Role Role, Guid? BarbershopId, string? PhotoUrl);
}
