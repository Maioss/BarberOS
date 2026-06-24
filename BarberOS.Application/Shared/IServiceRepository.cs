using BarberOS.Domain.Entities;

namespace BarberOS.Application.Shared
{
    public interface IServiceRepository
    {
        Task<Service?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<Service>> ListByBarbershopAsync(Guid barbershopId, bool includeInactive, CancellationToken ct = default);
        Task<IReadOnlyList<Service>> GetManyByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
        Task AddAsync(Service service, CancellationToken ct = default);
        void Update(Service service);
    }
}
