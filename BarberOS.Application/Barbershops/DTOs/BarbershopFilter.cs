using BarberOS.Application.Shared;

namespace BarberOS.Application.Barbershops.DTOs
{
    public record BarbershopFilter(
        int Page = 1,
        int PageSize = 20,
        string? City = null,
        bool? IsMain = null,
        bool? IsActive = null
    ) : PagedRequest(Page, PageSize);
}
