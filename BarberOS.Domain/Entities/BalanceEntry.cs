using BarberOS.Domain.Enums;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Domain.Entities
{
    /// <summary>
    /// Movimiento del saldo de un barbero. El saldo no se guarda como un numero que
    /// se suma y se resta: es la suma de estos asientos, que son inmutables. Asi no
    /// puede descuadrarse y queda el rastro de por que cambio.
    /// </summary>
    public class BalanceEntry
    {
        public Guid Id { get; private set; }
        public Guid BarberId { get; private set; }

        /// <summary>Positivo acredita, negativo descuenta.</summary>
        public decimal Amount { get; private set; }

        public BalanceEntryReason Reason { get; private set; }
        public Guid? AppointmentId { get; private set; }
        public Guid? PaymentId { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private BalanceEntry() { }

        private static BalanceEntry Create(
            Guid barberId,
            decimal amount,
            BalanceEntryReason reason,
            Guid? appointmentId,
            Guid? paymentId)
        {
            if (barberId == Guid.Empty)
                throw new BusinessRuleException("El movimiento debe pertenecer a un barbero.");

            if (amount == 0m)
                throw new BusinessRuleException("Un movimiento de saldo no puede ser de cero.");

            return new BalanceEntry
            {
                Id = Guid.NewGuid(),
                BarberId = barberId,
                Amount = amount,
                Reason = reason,
                AppointmentId = appointmentId,
                PaymentId = paymentId,
                CreatedAt = DateTime.UtcNow
            };
        }

        /// <summary>Acredita al barbero lo que valio la cita que atendio.</summary>
        public static BalanceEntry ForCompletedAppointment(Guid barberId, Guid appointmentId, decimal amount)
        {
            if (amount <= 0m)
                throw new BusinessRuleException("El valor de la cita debe ser positivo.");

            return Create(barberId, amount, BalanceEntryReason.AppointmentCompleted, appointmentId, null);
        }

        /// <summary>Descuenta un reembolso. Se guarda en negativo.</summary>
        public static BalanceEntry ForRefundedPayment(Guid barberId, Guid paymentId, Guid appointmentId, decimal amount)
        {
            if (amount <= 0m)
                throw new BusinessRuleException("El monto reembolsado debe ser positivo.");

            return Create(barberId, -amount, BalanceEntryReason.PaymentRefunded, appointmentId, paymentId);
        }

        /// <summary>Ajuste manual, para cuadrar a mano lo que ningun flujo cubre.</summary>
        public static BalanceEntry ForAdjustment(Guid barberId, decimal amount) =>
            Create(barberId, amount, BalanceEntryReason.Adjustment, null, null);
    }
}
