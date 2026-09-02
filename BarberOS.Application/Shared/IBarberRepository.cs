using BarberOS.Domain.Entities;

namespace BarberOS.Application.Shared
{
    public interface IBarberRepository
    {
        Task<Barber?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<Barber>> GetManyByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
        Task<Barber?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task<bool> ExistsByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task<IReadOnlyList<Barber>> ListByBarbershopAsync(Guid barbershopId, bool includeInactive, CancellationToken ct = default);
        Task AddAsync(Barber barber, CancellationToken ct = default);
        void Update(Barber barber);
    }
}
