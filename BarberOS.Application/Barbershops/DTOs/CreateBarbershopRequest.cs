namespace BarberOS.Application.Barbershops.DTOs
{
    public record CreateBarbershopRequest(
        string Name,
        string Address,
        string City,
        string? Phone,
        bool IsMain,
        Guid? ParentId
    );
}
