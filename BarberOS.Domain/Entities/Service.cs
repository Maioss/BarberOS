using BarberOS.Domain.Exceptions;

namespace BarberOS.Domain.Entities
{
    public class Service
    {
        public Guid Id { get; private set; }
        public Guid BarbershopId { get; private set; }
        public string Name { get; private set; } = null!;
        public string? Description { get; private set; }
        public decimal Price { get; private set; }
        public int DurationMinutes { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private Service() { }

        public static Service Create(Guid barbershopId, string name, string? description, decimal price, int durationMinutes)
        {
            ValidatePrice(price);
            ValidateDuration(durationMinutes);

            var now = DateTime.UtcNow;
            return new Service
            {
                Id = Guid.NewGuid(),
                BarbershopId = barbershopId,
                Name = name.Trim(),
                Description = description?.Trim(),
                Price = price,
                DurationMinutes = durationMinutes,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };
        }

        public void Update(string name, string? description, decimal price, int durationMinutes)
        {
            ValidatePrice(price);
            ValidateDuration(durationMinutes);

            Name = name.Trim();
            Description = description?.Trim();
            Price = price;
            DurationMinutes = durationMinutes;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        private static void ValidatePrice(decimal price)
        {
            if (price < 0)
                throw new BusinessRuleException("El precio no puede ser negativo.");
        }

        private static void ValidateDuration(int minutes)
        {
            if (minutes <= 0)
                throw new BusinessRuleException("La duración debe ser mayor a cero.");

            if (minutes % 5 != 0)
                throw new BusinessRuleException("La duración debe ser un múltiplo de 5 minutos.");
        }
    }
}
