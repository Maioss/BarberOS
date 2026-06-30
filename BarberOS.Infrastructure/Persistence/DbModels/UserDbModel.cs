namespace BarberOS.Infrastructure.Persistence.DbModels
{
    public class UserDbModel
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string? Phone { get; set; }
        public string? PhotoUrl { get; set; }
        public int Role { get; set; }
        public Guid? BarbershopId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
