using BarberOS.Domain.Enums;

namespace BarberOS.Application.Payments.DTOs
{
    public record PaymentDto(
        Guid Id,
        Guid AppointmentId,
        Guid ClientId,
        Guid BarberId,
        Guid BarbershopId,
        decimal Amount,
        PaymentMethod Method,
        PaymentStatus Status,
        string? Notes,
        DateTime? PaidAt,
        DateTime? RefundedAt,
        DateTime CreatedAt);
}
