using BarberOS.Application.Barbers.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Enums;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Barbers.UseCases
{
    public class GetAvailabilityUseCase
    {
        private static readonly TimeOnly WorkdayStart = new(9, 0);
        private static readonly TimeOnly WorkdayEnd = new(18, 0);
        private static readonly TimeSpan SlotDuration = TimeSpan.FromMinutes(30);

        private readonly IBarberRepository _barbers;
        private readonly IAppointmentRepository _appointments;

        public GetAvailabilityUseCase(IBarberRepository barbers, IAppointmentRepository appointments)
        {
            _barbers = barbers;
            _appointments = appointments;
        }

        public async Task<AvailabilityDto> ExecuteAsync(Guid barberId, DateOnly date, CancellationToken ct = default)
        {
            var barber = await _barbers.GetByIdAsync(barberId, ct)
                ?? throw NotFoundException.For("barbero", barberId);

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            if (date < today)
                throw new BusinessRuleException("No se puede consultar disponibilidad de fechas pasadas.");

            if (!barber.IsAvailableOn(date.DayOfWeek))
            {
                return new AvailabilityDto(
                    barber.Id, date, IsWorkingDay: false,
                    Slots: Array.Empty<SlotDto>(),
                    Message: "El barbero no trabaja este día."
                );
            }

            var existing = await _appointments.ListByBarberAndDateAsync(
                barber.Id, date, AppointmentStatus.Confirmed, ct);

            var bookedIntervals = existing.Select(a => (a.StartTime, a.EndTime)).ToList();
            var slots = BuildSlots(barber.LunchStart, barber.LunchEnd, bookedIntervals);

            return new AvailabilityDto(
                barber.Id, date, IsWorkingDay: true,
                Slots: slots,
                Message: null
            );
        }

        private static List<SlotDto> BuildSlots(
            TimeOnly lunchStart,
            TimeOnly lunchEnd,
            List<(TimeOnly Start, TimeOnly End)> bookedIntervals)
        {
            var slots = new List<SlotDto>();
            var cursor = WorkdayStart;

            while (cursor < WorkdayEnd)
            {
                var slotEnd = cursor.Add(SlotDuration);
                var overlapsLunch = cursor < lunchEnd && slotEnd > lunchStart;
                if (!overlapsLunch)
                {
                    var isBooked = bookedIntervals.Any(b => cursor < b.End && b.Start < slotEnd);
                    if (!isBooked)
                        slots.Add(new SlotDto(cursor, slotEnd));
                }
                cursor = slotEnd;
            }

            return slots;
        }
    }
}
