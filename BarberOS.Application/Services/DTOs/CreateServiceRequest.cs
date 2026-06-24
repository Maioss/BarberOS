namespace BarberOS.Application.Services.DTOs
{
    public record CreateServiceRequest(
        Guid BarbershopId,
        string Name,
        string? Description,
        decimal Price,
        int DurationMinutes
    );
}
