namespace BarberOS.Infrastructure.Persistence.DbModels
{
    public class PaymentDbModel
    {
        public Guid Id { get; set; }
        public Guid AppointmentId { get; set; }
        public Guid ClientId { get; set; }
        public Guid BarberId { get; set; }
        public Guid BarbershopId { get; set; }
        public decimal Amount { get; set; }
        public int Method { get; set; }
        public int Status { get; set; }
        public string? Notes { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? RefundedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public AppointmentDbModel Appointment { get; set; } = null!;
    }
}
