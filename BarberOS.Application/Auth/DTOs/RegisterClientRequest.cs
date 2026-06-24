namespace BarberOS.Application.Auth.DTOs
{
    public record RegisterClientRequest(string Email, string Password, string FullName, string? Phone);
}
