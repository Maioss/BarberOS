using BarberOS.Domain.Enums;

namespace BarberOS.Application.Payments.DTOs
{
    public record RegisterPaymentRequest(
        Guid AppointmentId,
        PaymentMethod Method,
        decimal? Amount = null,
        string? Notes = null);
}
