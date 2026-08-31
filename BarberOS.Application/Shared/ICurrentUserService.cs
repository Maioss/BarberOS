using BarberOS.Domain.Enums;

namespace BarberOS.Application.Shared
{

    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        Role? Role { get; }
        Guid? BarbershopId { get; }
        bool IsAuthenticated { get; }
    }
}
