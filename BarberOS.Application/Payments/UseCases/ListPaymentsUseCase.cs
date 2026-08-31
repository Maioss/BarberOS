using BarberOS.Application.Payments.DTOs;
using BarberOS.Application.Shared;

namespace BarberOS.Application.Payments.UseCases
{
    public class ListPaymentsUseCase
    {
        private readonly IPaymentRepository _payments;
        private readonly TenantScope _scope;

        public ListPaymentsUseCase(IPaymentRepository payments, TenantScope scope)
        {
            _payments = payments;
            _scope = scope;
        }

        public async Task<PagedResult<PaymentDto>> ExecuteAsync(PaymentFilter filter, CancellationToken ct = default)
        {
            // null = SuperAdmin, sin restriccion de sede.
            var allowed = await _scope.VisibleSiteIdsAsync(ct);
            var sites = allowed;

            if (filter.BarbershopId is not null)
            {
                await _scope.EnsureInScopeAsync(filter.BarbershopId.Value, ct);

                var requested = await _scope.SitesCoveredByAsync(filter.BarbershopId.Value, ct);
                sites = allowed is null ? requested : requested.Where(allowed.Contains).ToList();
            }

            var scoped = filter with { BarbershopId = null, BarbershopIds = sites };

            var result = await _payments.ListAsync(scoped, ct);
            return new PagedResult<PaymentDto>(
                result.Items.Select(RegisterPaymentUseCase.MapToDto).ToList(),
                result.Page, result.PageSize, result.TotalCount);
        }
    }
}
