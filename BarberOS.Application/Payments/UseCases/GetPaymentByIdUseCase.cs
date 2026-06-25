using BarberOS.Application.Payments.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Payments.UseCases
{
    public class GetPaymentByIdUseCase
    {
        private readonly IPaymentRepository _payments;

        public GetPaymentByIdUseCase(IPaymentRepository payments) => _payments = payments;

        public async Task<PaymentDto> ExecuteAsync(Guid id, CancellationToken ct = default)
        {
            var payment = await _payments.GetByIdAsync(id, ct)
                ?? throw NotFoundException.For("pago", id);

            return RegisterPaymentUseCase.MapToDto(payment);
        }
    }
}
