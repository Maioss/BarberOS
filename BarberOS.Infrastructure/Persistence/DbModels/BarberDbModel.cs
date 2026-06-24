namespace BarberOS.Infrastructure.Persistence.DbModels
{
    public class BarberDbModel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid BarbershopId { get; set; }
        public TimeOnly LunchStart { get; set; }
        public TimeOnly LunchEnd { get; set; }
        public int AvailableDays { get; set; }
        public decimal Balance { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
