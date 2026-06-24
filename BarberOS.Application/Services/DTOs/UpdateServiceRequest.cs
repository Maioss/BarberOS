namespace BarberOS.Application.Services.DTOs
{
    public record UpdateServiceRequest(
        string Name,
        string? Description,
        decimal Price,
        int DurationMinutes
    );
}
