namespace BarberOS.Infrastructure.Persistence.DbModels
{
    public class AppointmentDbModel
    {
        public Guid Id { get; set; }
        public Guid ClientId { get; set; }
        public Guid BarberId { get; set; }
        public Guid BarbershopId { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public decimal TotalPrice { get; set; }
        public int Status { get; set; }
        public string? Notes { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public List<AppointmentServiceDbModel> Services { get; set; } = new();
    }
}
