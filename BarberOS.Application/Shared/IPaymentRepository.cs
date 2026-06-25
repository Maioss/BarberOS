using BarberOS.Application.Payments.DTOs;
using BarberOS.Domain.Entities;
using BarberOS.Domain.Enums;

namespace BarberOS.Application.Shared
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Payment?> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken ct = default);
        Task<PagedResult<Payment>> ListAsync(PaymentFilter filter, CancellationToken ct = default);
        Task AddAsync(Payment payment, CancellationToken ct = default);
        void Update(Payment payment);
    }
}
