namespace BarberOS.Domain.Entities
{
    public class Barbershop
    {
        public const string DefaultTimeZoneId = "America/Bogota";

        public Guid Id { get; private set; }
        public string Name { get; private set; } = null!;
        public string Address { get; private set; } = null!;
        public string City { get; private set; } = null!;
        public string? Phone { get; private set; }

        /// <summary>Identificador IANA, no de Windows.</summary>
        public string TimeZoneId { get; private set; } = DefaultTimeZoneId;

        public bool IsMain { get; private set; }
        public Guid? ParentId { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private Barbershop() { }

        public static Barbershop CreateMain(string name, string address, string city, string? phone)
        {
            return new Barbershop
            {
                Id = Guid.NewGuid(),
                Name = name,
                Address = address,
                City = city,
                Phone = phone,
                TimeZoneId = DefaultTimeZoneId,
                IsMain = true,
                ParentId = null,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static Barbershop CreateBranch(string name, string address, string city, string? phone, Guid parentId)
        {
            return new Barbershop
            {
                Id = Guid.NewGuid(),
                Name = name,
                Address = address,
                City = city,
                Phone = phone,
                TimeZoneId = DefaultTimeZoneId,
                IsMain = false,
                ParentId = parentId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public void UpdateInfo(string name, string address, string city, string? phone)
        {
            Name = name;
            Address = address;
            City = city;
            Phone = phone;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
