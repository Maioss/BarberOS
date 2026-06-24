using BarberOS.Application.Shared;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Services.UseCases
{
    public class DeleteServiceUseCase
    {
        private readonly IServiceRepository _services;
        private readonly IUnitOfWork _uow;

        public DeleteServiceUseCase(IServiceRepository services, IUnitOfWork uow)
        {
            _services = services;
            _uow = uow;
        }

        public async Task ExecuteAsync(Guid id, CancellationToken ct = default)
        {
            var s = await _services.GetByIdAsync(id, ct)
                ?? throw NotFoundException.For("servicio", id);

            s.Deactivate();
            _services.Update(s);
            await _uow.SaveChangesAsync(ct);
        }
    }
}
