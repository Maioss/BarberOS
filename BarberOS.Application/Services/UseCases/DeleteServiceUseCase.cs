using BarberOS.Application.Shared;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Services.UseCases
{
    public class DeleteServiceUseCase
    {
        private readonly IServiceRepository _services;
        private readonly TenantScope _scope;
        private readonly IUnitOfWork _uow;

        public DeleteServiceUseCase(IServiceRepository services, TenantScope scope, IUnitOfWork uow)
        {
            _services = services;
            _scope = scope;
            _uow = uow;
        }

        public async Task ExecuteAsync(Guid id, CancellationToken ct = default)
        {
            var s = await _services.GetByIdAsync(id, ct)
                ?? throw NotFoundException.For("servicio", id);

            await _scope.EnsureInScopeAsync(s.BarbershopId, ct);

            s.Deactivate();
            _services.Update(s);
            await _uow.SaveChangesAsync(ct);
        }
    }
}
