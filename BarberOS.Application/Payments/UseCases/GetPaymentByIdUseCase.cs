using BarberOS.Application.Payments.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Payments.UseCases
{
    public class GetPaymentByIdUseCase
    {
        private readonly IPaymentRepository _payments;
        private readonly TenantScope _scope;

        public GetPaymentByIdUseCase(IPaymentRepository payments, TenantScope scope)
        {
            _payments = payments;
            _scope = scope;
        }

        public async Task<PaymentDto> ExecuteAsync(Guid id, CancellationToken ct = default)
        {
            var payment = await _payments.GetByIdAsync(id, ct)
                ?? throw NotFoundException.For("pago", id);

            await _scope.EnsureInScopeAsync(payment.BarbershopId, ct);

            return RegisterPaymentUseCase.MapToDto(payment);
        }
    }
}
