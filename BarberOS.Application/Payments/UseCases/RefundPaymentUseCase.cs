using BarberOS.Application.Shared;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Payments.UseCases
{
    public class RefundPaymentUseCase
    {
        private readonly IPaymentRepository _payments;
        private readonly IBarberRepository _barbers;
        private readonly TenantScope _scope;
        private readonly IUnitOfWork _uow;

        public RefundPaymentUseCase(
            IPaymentRepository payments,
            IBarberRepository barbers,
            TenantScope scope,
            IUnitOfWork uow)
        {
            _payments = payments;
            _barbers = barbers;
            _scope = scope;
            _uow = uow;
        }

        public async Task ExecuteAsync(Guid paymentId, CancellationToken ct = default)
        {
            var payment = await _payments.GetByIdAsync(paymentId, ct)
                ?? throw NotFoundException.For("pago", paymentId);

            await _scope.EnsureInScopeAsync(payment.BarbershopId, ct);

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
