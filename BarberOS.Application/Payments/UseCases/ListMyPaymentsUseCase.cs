using BarberOS.Application.Payments.DTOs;
using BarberOS.Application.Shared;

namespace BarberOS.Application.Payments.UseCases
{
    public class ListMyPaymentsUseCase
    {
        private readonly IPaymentRepository _payments;
        private readonly ICurrentUserService _current;

        public ListMyPaymentsUseCase(IPaymentRepository payments, ICurrentUserService current)
        {
            _payments = payments;
            _current = current;
        }

        public async Task<PagedResult<PaymentDto>> ExecuteAsync(PaymentFilter filter, CancellationToken ct = default)
        {
            var result = await _payments.ListAsync(filter with { ClientId = _current.RequireUserId() }, ct);
            return new PagedResult<PaymentDto>(
                result.Items.Select(RegisterPaymentUseCase.MapToDto).ToList(),
                result.Page, result.PageSize, result.TotalCount);
        }
    }
}
