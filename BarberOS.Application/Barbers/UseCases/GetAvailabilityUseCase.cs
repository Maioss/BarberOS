using BarberOS.Application.Barbers.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Barbers.UseCases
{
    public class GetAvailabilityUseCase
    {
        private static readonly TimeOnly WorkdayStart = new(9, 0);
        private static readonly TimeOnly WorkdayEnd = new(18, 0);
        private static readonly TimeSpan SlotDuration = TimeSpan.FromMinutes(30);

        private readonly IBarberRepository _barbers;

        public GetAvailabilityUseCase(IBarberRepository barbers) => _barbers = barbers;

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

            var slots = BuildBaseSlots(barber.LunchStart, barber.LunchEnd);

            return new AvailabilityDto(
                barber.Id, date, IsWorkingDay: true,
                Slots: slots,
                Message: null
            );
        }

        private static List<SlotDto> BuildBaseSlots(TimeOnly lunchStart, TimeOnly lunchEnd)
        {
            var slots = new List<SlotDto>();
            var cursor = WorkdayStart;

            while (cursor < WorkdayEnd)
            {
                var slotEnd = cursor.Add(SlotDuration);
                var overlapsLunch = cursor < lunchEnd && slotEnd > lunchStart;
                if (!overlapsLunch)
                    slots.Add(new SlotDto(cursor, slotEnd));
                cursor = slotEnd;
            }

            return slots;
        }
    }
}
