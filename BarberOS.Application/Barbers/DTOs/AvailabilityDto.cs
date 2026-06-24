namespace BarberOS.Application.Barbers.DTOs
{
    public record AvailabilityDto(
        Guid BarberId,
        DateOnly Date,
        bool IsWorkingDay,
        IReadOnlyList<SlotDto> Slots,
        string? Message
    );
}
