using BarberOS.Domain.Enums;

namespace BarberOS.Application.Shared
{

    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        Role? Role { get; }
        bool IsAuthenticated { get; }
    }
}
