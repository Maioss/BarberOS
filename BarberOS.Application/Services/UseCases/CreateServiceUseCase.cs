using BarberOS.Application.Services.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Entities;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Services.UseCases
{
    public class CreateServiceUseCase
    {
        private readonly IServiceRepository _services;
        private readonly IBarbershopRepository _shops;
        private readonly TenantScope _scope;
        private readonly IUnitOfWork _uow;

        public CreateServiceUseCase(
            IServiceRepository services,
            IBarbershopRepository shops,
            TenantScope scope,
            IUnitOfWork uow)
        {
            _services = services;
            _shops = shops;
            _scope = scope;
            _uow = uow;
        }

        public async Task<ServiceDto> ExecuteAsync(CreateServiceRequest request, CancellationToken ct = default)
        {
            var shop = await _shops.GetByIdAsync(request.BarbershopId, ct)
                ?? throw NotFoundException.For("barbería", request.BarbershopId);

            await _scope.EnsureInScopeAsync(shop.Id, ct);

            if (!shop.IsActive)
                throw new BusinessRuleException("La barbería está desactivada.");

            if (!shop.IsMain)
                throw new BusinessRuleException("Los servicios solo se definen en barberías principales, no en sedes.");

            var service = Service.Create(shop.Id, request.Name, request.Description, request.Price, request.DurationMinutes);

            await _services.AddAsync(service, ct);
            await _uow.SaveChangesAsync(ct);

            return new ServiceDto(
                service.Id, service.BarbershopId, service.Name, service.Description,
                service.Price, service.DurationMinutes, service.IsActive
            );
        }
    }
}
