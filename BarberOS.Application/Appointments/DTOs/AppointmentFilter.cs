using BarberOS.Application.Shared;
using BarberOS.Domain.Enums;

namespace BarberOS.Application.Appointments.DTOs
{
    public record AppointmentFilter(
        int Page = 1,
        int PageSize = 20,
        Guid? BarberId = null,
        Guid? BarbershopId = null,
        Guid? ClientId = null,
        AppointmentStatus? Status = null,
        DateOnly? DateFrom = null,
        DateOnly? DateTo = null,
        // Lo fija el servidor a partir del ambito del usuario; nunca se toma del cliente.
        IReadOnlyList<Guid>? BarbershopIds = null
    ) : PagedRequest(Page, PageSize);
}
