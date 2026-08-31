using BarberOS.Domain.Enums;

namespace BarberOS.Application.Payments.DTOs
{
    /// <summary>El monto no se pide: sale del total de la cita.</summary>
    public record RegisterPaymentRequest(
        Guid AppointmentId,
        PaymentMethod Method,
        string? Notes = null);
}
