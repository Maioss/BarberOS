using BarberOS.Application.Payments.DTOs;
using BarberOS.Application.Shared;

namespace BarberOS.Application.Payments.UseCases
{
    public class ListPaymentsUseCase
    {
        private readonly IPaymentRepository _payments;

        public ListPaymentsUseCase(IPaymentRepository payments) => _payments = payments;

        public async Task<PagedResult<PaymentDto>> ExecuteAsync(PaymentFilter filter, CancellationToken ct = default)
        {
            var result = await _payments.ListAsync(filter, ct);
            return new PagedResult<PaymentDto>(
                result.Items.Select(RegisterPaymentUseCase.MapToDto).ToList(),
                result.Page, result.PageSize, result.TotalCount);
        }
    }
}
