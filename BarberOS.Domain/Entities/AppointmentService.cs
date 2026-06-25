namespace BarberOS.Domain.Entities
{
    public class AppointmentService
    {
        public Guid Id { get; private set; }
        public Guid AppointmentId { get; private set; }
        public Guid ServiceId { get; private set; }
        public string ServiceName { get; private set; } = null!;
        public decimal Price { get; private set; }
        public int DurationMinutes { get; private set; }

        private AppointmentService() { }

        internal static AppointmentService Create(Guid appointmentId, Service service)
        {
            return new AppointmentService
            {
                Id = Guid.NewGuid(),
                AppointmentId = appointmentId,
                ServiceId = service.Id,
                ServiceName = service.Name,
                Price = service.Price,
                DurationMinutes = service.DurationMinutes
            };
        }
    }
}
