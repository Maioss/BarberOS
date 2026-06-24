namespace BarberOS.Application.Services.DTOs
{
    public record ServiceDto(
        Guid Id,
        Guid BarbershopId,
        string Name,
        string? Description,
        decimal Price,
        int DurationMinutes,
        bool IsActive
    );
}
