namespace BarberOS.Application.Barbershops.DTOs
{
    public record UpdateBarbershopRequest(
        string Name,
        string Address,
        string City,
        string? Phone
    );
}
