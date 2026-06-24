using BarberOS.Application.Services.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Services.UseCases
{
    public class ListServicesByBarbershopUseCase
    {
        private readonly IServiceRepository _services;
        private readonly IBarbershopRepository _shops;

        public ListServicesByBarbershopUseCase(IServiceRepository services, IBarbershopRepository shops)
        {
            _services = services;
            _shops = shops;
        }

        public async Task<IReadOnlyList<ServiceDto>> ExecuteAsync(Guid barbershopId, CancellationToken ct = default)
        {
            var shop = await _shops.GetByIdAsync(barbershopId, ct)
                ?? throw NotFoundException.For("barbería", barbershopId);

            var targetId = shop.IsMain ? shop.Id : shop.ParentId!.Value;

            var services = await _services.ListByBarbershopAsync(targetId, includeInactive: false, ct);

            return services.Select(s => new ServiceDto(
                s.Id, s.BarbershopId, s.Name, s.Description, s.Price, s.DurationMinutes, s.IsActive
            )).ToList();
        }
    }
}
