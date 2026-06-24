using BarberOS.Application.Shared;
using BarberOS.Domain.Enums;

namespace BarberOS.Application.Users.DTOs
{
    public record UserFilter(
        Role? Role = null,
        Guid? BarbershopId = null,
        bool? IsActive = null,
        string? Search = null,
        int Page = 1,
        int PageSize = 20
    ) : PagedRequest(Page, PageSize);
}
