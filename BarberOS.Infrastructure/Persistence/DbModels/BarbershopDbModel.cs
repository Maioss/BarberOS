namespace BarberOS.Infrastructure.Persistence.DbModels
{
    public class BarbershopDbModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string City { get; set; } = null!;
        public string? Phone { get; set; }
        public string TimeZoneId { get; set; } = "America/Bogota";
        public bool IsMain { get; set; }
        public Guid? ParentId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public BarbershopDbModel? Parent { get; set; }
        public ICollection<BarbershopDbModel> Branches { get; set; } = new List<BarbershopDbModel>();
    }
}
