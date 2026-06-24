using BarberOS.Application.Users.DTOs;
using BarberOS.Domain.Entities;

namespace BarberOS.Application.Shared
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
        Task<PagedResult<User>> ListAsync(UserFilter filter, CancellationToken ct = default);
        Task AddAsync(User user, CancellationToken ct = default);
        void Update(User user);
    }
}
