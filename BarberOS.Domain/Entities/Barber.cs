using BarberOS.Domain.Exceptions;

namespace BarberOS.Domain.Entities
{
    public class Barber
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public Guid BarbershopId { get; private set; }
        public TimeOnly LunchStart { get; private set; }
        public TimeOnly LunchEnd { get; private set; }
        public int AvailableDays { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private Barber() { }

        public static Barber Create(Guid userId, Guid barbershopId)
        {
            var now = DateTime.UtcNow;
            return new Barber
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                BarbershopId = barbershopId,
                LunchStart = new TimeOnly(12, 0),
                LunchEnd = new TimeOnly(13, 0),
                AvailableDays = 0b1111110,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };
        }

        public void UpdateSchedule(TimeOnly lunchStart, TimeOnly lunchEnd, IEnumerable<DayOfWeek> availableDays)
        {
            if (lunchStart >= lunchEnd)
                throw new BusinessRuleException("La hora de inicio del almuerzo debe ser anterior a la de fin.");

            if (lunchStart < new TimeOnly(9, 0) || lunchEnd > new TimeOnly(18, 0))
                throw new BusinessRuleException("El almuerzo debe estar dentro del horario laboral (09:00 a 18:00).");

            var days = availableDays.Distinct().ToList();
            if (days.Count == 0)
                throw new BusinessRuleException("Debes estar disponible al menos un día a la semana.");

            LunchStart = lunchStart;
            LunchEnd = lunchEnd;
            AvailableDays = ToBitmask(days);
            UpdatedAt = DateTime.UtcNow;
        }

        public bool IsAvailableOn(DayOfWeek day) => (AvailableDays & (1 << (int)day)) != 0;

        public IReadOnlyList<DayOfWeek> GetAvailableDays()
        {
            var result = new List<DayOfWeek>();
            for (int i = 0; i < 7; i++)
                if ((AvailableDays & (1 << i)) != 0)
                    result.Add((DayOfWeek)i);
            return result;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        private static int ToBitmask(IEnumerable<DayOfWeek> days)
        {
            int mask = 0;
            foreach (var d in days) mask |= 1 << (int)d;
            return mask;
        }
    }
}
