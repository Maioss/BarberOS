namespace BarberOS.Application.Barbers.DTOs
{
    public record OnboardBarberRequest(
        string Email,
        string Password,
        string FullName,
        string? Phone,
        Guid BarbershopId);
}
