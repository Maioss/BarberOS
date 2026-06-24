namespace BarberOS.Application.Barbershops.DTOs
{
    public record BarbershopDto(
        Guid Id,
        string Name,
        string Address,
        string City,
        string? Phone,
        bool IsMain,
        Guid? ParentId,
        bool IsActive,
        DateTime CreatedAt,
        DateTime UpdatedAt
    );
}
