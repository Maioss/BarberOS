using BarberOS.Application.Payments.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Entities;
using BarberOS.Domain.Enums;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Payments.UseCases
{
    public class RegisterPaymentUseCase
    {
        private readonly IPaymentRepository _payments;
        private readonly IAppointmentRepository _appointments;
        private readonly IUnitOfWork _uow;

        public RegisterPaymentUseCase(
            IPaymentRepository payments,
            IAppointmentRepository appointments,
            IUnitOfWork uow)
        {
            _payments = payments;
            _appointments = appointments;
            _uow = uow;
        }

        public async Task<PaymentDto> ExecuteAsync(RegisterPaymentRequest request, CancellationToken ct = default)
        {
            var appointment = await _appointments.GetByIdAsync(request.AppointmentId, ct)
                ?? throw NotFoundException.For("reserva", request.AppointmentId);

            if (appointment.Status != AppointmentStatus.Completed)
                throw new BusinessRuleException("Solo se pueden registrar pagos para reservas completadas.");

            var existing = await _payments.GetByAppointmentIdAsync(request.AppointmentId, ct);
            if (existing is not null && existing.Status == PaymentStatus.Paid)
                throw new ConflictException("Esta reserva ya tiene un pago registrado.");

            var payment = Payment.Create(
                appointment.Id,
                appointment.ClientId,
                appointment.BarberId,
                appointment.BarbershopId,
                appointment.TotalPrice,
                request.Method,
                request.Notes);

            await _payments.AddAsync(payment, ct);
            await _uow.SaveChangesAsync(ct);

            return MapToDto(payment);
        }

        internal static PaymentDto MapToDto(Payment p) => new(
            p.Id,
            p.AppointmentId,
            p.ClientId,
            p.BarberId,
            p.BarbershopId,
            p.Amount,
            p.Method,
            p.Status,
            p.Notes,
            p.PaidAt,
            p.RefundedAt,
            p.CreatedAt);
    }
}
