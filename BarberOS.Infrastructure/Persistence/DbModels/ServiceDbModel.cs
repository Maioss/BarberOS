namespace BarberOS.Infrastructure.Persistence.DbModels
{
    public class ServiceDbModel
    {
        public Guid Id { get; set; }
        public Guid BarbershopId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
