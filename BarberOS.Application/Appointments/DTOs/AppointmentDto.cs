using BarberOS.Domain.Enums;

namespace BarberOS.Application.Appointments.DTOs
{
    public record AppointmentDto(
        Guid Id,
        Guid ClientId,
        Guid BarberId,
        Guid BarbershopId,
        DateOnly Date,
        TimeOnly StartTime,
        TimeOnly EndTime,
        decimal TotalPrice,
        AppointmentStatus Status,
        string? Notes,
        DateTime? CompletedAt,
        DateTime? CancelledAt,
        DateTime CreatedAt,
        IReadOnlyList<AppointmentServiceDto> Services);
}
