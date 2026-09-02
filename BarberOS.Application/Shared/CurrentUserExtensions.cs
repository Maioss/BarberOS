using BarberOS.Domain.Enums;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Shared
{
    public static class CurrentUserExtensions
    {
        public static Guid RequireUserId(this ICurrentUserService current) =>
            current.UserId ?? throw new UnauthorizedException("No autenticado.");

        public static Role RequireRole(this ICurrentUserService current) =>
            current.Role ?? throw new UnauthorizedException("No autenticado.");
    }
}
