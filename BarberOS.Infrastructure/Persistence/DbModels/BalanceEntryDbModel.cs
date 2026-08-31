namespace BarberOS.Infrastructure.Persistence.DbModels
{
    public class BalanceEntryDbModel
    {
        public Guid Id { get; set; }
        public Guid BarberId { get; set; }
        public decimal Amount { get; set; }
        public int Reason { get; set; }
        public Guid? AppointmentId { get; set; }
        public Guid? PaymentId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
