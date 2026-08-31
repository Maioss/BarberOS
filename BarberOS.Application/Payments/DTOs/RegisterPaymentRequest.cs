using BarberOS.Domain.Enums;

namespace BarberOS.Application.Payments.DTOs
{
    /// <summary>
    /// El monto no se pide: sale del total de la cita. Cuando lo mandaba el cliente,
    /// un pago podia no tener ninguna relacion con lo reservado y el reembolso
    /// descontaba del saldo del barbero algo que nunca se le habia acreditado.
    /// </summary>
    public record RegisterPaymentRequest(
        Guid AppointmentId,
        PaymentMethod Method,
        string? Notes = null);
}
