using BarberOS.Application.Shared;
using BarberOS.Domain.Enums;

namespace BarberOS.Application.Payments.DTOs
{
    public record PaymentFilter(
        int Page = 1,
        int PageSize = 20,
        Guid? AppointmentId = null,
        Guid? ClientId = null,
        Guid? BarberId = null,
        Guid? BarbershopId = null,
        PaymentStatus? Status = null,
        DateOnly? DateFrom = null,
        DateOnly? DateTo = null,
        // Lo fija el servidor a partir del ambito del usuario; nunca se toma del cliente.
        IReadOnlyList<Guid>? BarbershopIds = null
    ) : PagedRequest(Page, PageSize);
}
