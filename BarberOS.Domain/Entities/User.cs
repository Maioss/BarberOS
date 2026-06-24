using BarberOS.Domain.Enums;

namespace BarberOS.Domain.Entities
{

    public class User
    {
        public Guid Id { get; private set; }
        public string Email { get; private set; } = null!;
        public string PasswordHash { get; private set; } = null!;
        public string FullName { get; private set; } = null!;
        public string? Phone { get; private set; }
        public Role Role { get; private set; }
        public Guid? BarbershopId { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private User() { }

        public static User Create(string email, string passwordHash, string fullName, Role role, string? phone = null, Guid? barbershopId = null)
        {
            var now = DateTime.UtcNow;
            return new User
            {
                Id = Guid.NewGuid(),
                Email = email.Trim().ToLowerInvariant(),
                PasswordHash = passwordHash,
                FullName = fullName.Trim(),
                Phone = phone?.Trim(),
                Role = role,
                BarbershopId = barbershopId,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now
            };
        }

        public void UpdateProfile(string fullName, string? phone)
        {
            FullName = fullName.Trim();
            Phone = phone?.Trim();
            UpdatedAt = DateTime.UtcNow;
        }

        public void ChangePassword(string newPasswordHash)
        {
            PasswordHash = newPasswordHash;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
