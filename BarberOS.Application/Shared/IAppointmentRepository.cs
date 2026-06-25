using BarberOS.Application.Appointments.DTOs;
using BarberOS.Domain.Entities;
using BarberOS.Domain.Enums;

namespace BarberOS.Application.Shared
{
    public interface IAppointmentRepository
    {
        Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<Appointment>> ListByBarberAndDateAsync(Guid barberId, DateOnly date, AppointmentStatus status, CancellationToken ct = default);
        Task<PagedResult<Appointment>> ListAsync(AppointmentFilter filter, CancellationToken ct = default);
        Task<PagedResult<Appointment>> ListByClientAsync(Guid clientId, AppointmentFilter filter, CancellationToken ct = default);
        Task<bool> ClientHasConflictingAppointmentAsync(Guid clientId, DateOnly date, TimeOnly start, TimeOnly end, CancellationToken ct = default);
        Task AddAsync(Appointment appointment, CancellationToken ct = default);
        void Update(Appointment appointment);
    }
}
