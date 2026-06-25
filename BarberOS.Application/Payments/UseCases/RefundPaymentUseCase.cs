using BarberOS.Application.Shared;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Payments.UseCases
{
    public class RefundPaymentUseCase
    {
        private readonly IPaymentRepository _payments;
        private readonly IBarberRepository _barbers;
        private readonly IUnitOfWork _uow;

        public RefundPaymentUseCase(
            IPaymentRepository payments,
            IBarberRepository barbers,
            IUnitOfWork uow)
        {
            _payments = payments;
            _barbers = barbers;
            _uow = uow;
        }

        public async Task ExecuteAsync(Guid paymentId, CancellationToken ct = default)
        {
            var payment = await _payments.GetByIdAsync(paymentId, ct)
                ?? throw NotFoundException.For("pago", paymentId);

            var barber = await _barbers.GetByIdAsync(payment.BarberId, ct)
                ?? throw NotFoundException.For("barbero", payment.BarberId);

            payment.Refund();
            barber.DeductFromBalance(payment.Amount);

            _payments.Update(payment);
            _barbers.Update(barber);
            await _uow.SaveChangesAsync(ct);
        }
    }
}
