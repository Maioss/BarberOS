using BarberOS.Application.Services.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Services.UseCases
{
    public class UpdateServiceUseCase
    {
        private readonly IServiceRepository _services;
        private readonly IUnitOfWork _uow;

        public UpdateServiceUseCase(IServiceRepository services, IUnitOfWork uow)
        {
            _services = services;
            _uow = uow;
        }

        public async Task<ServiceDto> ExecuteAsync(Guid id, UpdateServiceRequest request, CancellationToken ct = default)
        {
            var s = await _services.GetByIdAsync(id, ct)
                ?? throw NotFoundException.For("servicio", id);

            s.Update(request.Name, request.Description, request.Price, request.DurationMinutes);
            _services.Update(s);
            await _uow.SaveChangesAsync(ct);

            return new ServiceDto(s.Id, s.BarbershopId, s.Name, s.Description, s.Price, s.DurationMinutes, s.IsActive);
        }
    }
}
