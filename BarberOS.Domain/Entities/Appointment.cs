using BarberOS.Domain.Enums;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Domain.Entities
{
    public class Appointment
    {
        public Guid Id { get; private set; }
        public Guid ClientId { get; private set; }
        public Guid BarberId { get; private set; }
        public Guid BarbershopId { get; private set; }
        public DateOnly Date { get; private set; }
        public TimeOnly StartTime { get; private set; }
        public TimeOnly EndTime { get; private set; }
        public decimal TotalPrice { get; private set; }
        public AppointmentStatus Status { get; private set; }
        public string? Notes { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public DateTime? CancelledAt { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private readonly List<AppointmentService> _services = new();
        public IReadOnlyList<AppointmentService> Services => _services.AsReadOnly();

        private Appointment() { }

        public static Appointment Create(
            Guid clientId,
            Guid barberId,
            Guid barbershopId,
            DateOnly date,
            TimeOnly startTime,
            IReadOnlyList<Service> services,
            string? notes = null)
        {
            if (services.Count == 0)
                throw new BusinessRuleException("Una reserva debe incluir al menos un servicio.");

            var totalMinutes = services.Sum(s => s.DurationMinutes);
            var endTime = startTime.AddMinutes(totalMinutes);
            var totalPrice = services.Sum(s => s.Price);

            var now = DateTime.UtcNow;
            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                BarberId = barberId,
                BarbershopId = barbershopId,
                Date = date,
                StartTime = startTime,
                EndTime = endTime,
                TotalPrice = totalPrice,
                Status = AppointmentStatus.Confirmed,
                Notes = notes?.Trim(),
                CreatedAt = now,
                UpdatedAt = now
            };

            foreach (var s in services)
                appointment._services.Add(AppointmentService.Create(appointment.Id, s));

            return appointment;
        }

        public void Complete()
        {
            if (Status == AppointmentStatus.Completed)
                throw new ConflictException("La reserva ya está marcada como completada.");

            if (Status == AppointmentStatus.Cancelled)
                throw new BusinessRuleException("No se puede completar una reserva cancelada.");

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (Date > today)
                throw new BusinessRuleException("No se puede completar una reserva futura.");

            Status = AppointmentStatus.Completed;
            CompletedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Cancel()
        {
            if (Status == AppointmentStatus.Cancelled)
                throw new ConflictException("La reserva ya está cancelada.");

            if (Status == AppointmentStatus.Completed)
                throw new BusinessRuleException("No se puede cancelar una reserva ya completada.");

            Status = AppointmentStatus.Cancelled;
            CancelledAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public bool OverlapsWith(TimeOnly otherStart, TimeOnly otherEnd) =>
            StartTime < otherEnd && otherStart < EndTime;
    }
}
