using BarberOS.Domain.Enums;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Domain.Entities
{
    public class Payment
    {
        public Guid Id { get; private set; }
        public Guid AppointmentId { get; private set; }
        public Guid ClientId { get; private set; }
        public Guid BarberId { get; private set; }
        public Guid BarbershopId { get; private set; }
        public decimal Amount { get; private set; }
        public PaymentMethod Method { get; private set; }
        public PaymentStatus Status { get; private set; }
        public string? Notes { get; private set; }
        public DateTime? PaidAt { get; private set; }
        public DateTime? RefundedAt { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private Payment() { }

        public static Payment Create(
            Guid appointmentId,
            Guid clientId,
            Guid barberId,
            Guid barbershopId,
            decimal amount,
            PaymentMethod method,
            string? notes = null)
        {
            if (amount <= 0)
                throw new BusinessRuleException("El monto del pago debe ser positivo.");

            var now = DateTime.UtcNow;
            return new Payment
            {
                Id = Guid.NewGuid(),
                AppointmentId = appointmentId,
                ClientId = clientId,
                BarberId = barberId,
                BarbershopId = barbershopId,
                Amount = amount,
                Method = method,
                Status = PaymentStatus.Paid,
                Notes = notes?.Trim(),
                PaidAt = now,
                CreatedAt = now,
                UpdatedAt = now
            };
        }

        public void Refund()
        {
            if (Status == PaymentStatus.Refunded)
                throw new ConflictException("El pago ya fue reembolsado.");

            if (Status != PaymentStatus.Paid)
                throw new BusinessRuleException("Solo se pueden reembolsar pagos con estado Paid.");

            Status = PaymentStatus.Refunded;
            RefundedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
