namespace BarberOS.Infrastructure.Persistence.DbModels
{
    public class AppointmentServiceDbModel
    {
        public Guid Id { get; set; }
        public Guid AppointmentId { get; set; }
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; } = null!;
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }

        public AppointmentDbModel Appointment { get; set; } = null!;
    }
}
