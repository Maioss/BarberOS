using BarberOS.Application.Payments.DTOs;
using BarberOS.Application.Shared;

namespace BarberOS.Application.Payments.UseCases
{
    public class ListMyPaymentsUseCase
    {
        private readonly IPaymentRepository _payments;

        public ListMyPaymentsUseCase(IPaymentRepository payments) => _payments = payments;

        public async Task<PagedResult<PaymentDto>> ExecuteAsync(Guid clientId, PaymentFilter filter, CancellationToken ct = default)
        {
            var result = await _payments.ListAsync(filter with { ClientId = clientId }, ct);
            return new PagedResult<PaymentDto>(
                result.Items.Select(RegisterPaymentUseCase.MapToDto).ToList(),
                result.Page, result.PageSize, result.TotalCount);
        }
    }
}
