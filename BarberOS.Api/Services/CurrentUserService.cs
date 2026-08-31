using System.Security.Claims;
using BarberOS.Application.Shared;
using BarberOS.Domain.Enums;

namespace BarberOS.Api.Services
{

    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _accessor;

        public CurrentUserService(IHttpContextAccessor accessor) => _accessor = accessor;

        public Guid? UserId
        {
            get
            {
                var sub = _accessor.HttpContext?.User.FindFirstValue("sub")
                       ?? _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
                return Guid.TryParse(sub, out var id) ? id : null;
            }
        }

        public Role? Role
        {
            get
            {
                var role = _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
                return Enum.TryParse<Role>(role, out var parsed) ? parsed : null;
            }
        }

        public Guid? BarbershopId
        {
            get
            {
                var id = _accessor.HttpContext?.User.FindFirstValue("barbershopId");
                return Guid.TryParse(id, out var parsed) ? parsed : null;
            }
        }

        public bool IsAuthenticated => _accessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
    }
}
