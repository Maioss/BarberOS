using BarberOS.Application.Shared;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Barbershops.UseCases
{
    public class DeleteBarbershopUseCase
    {
        private readonly IBarbershopRepository _repo;
        private readonly IUnitOfWork _uow;

        public DeleteBarbershopUseCase(IBarbershopRepository repo, IUnitOfWork uow)
        {
            _repo = repo;
            _uow = uow;
        }

        public async Task ExecuteAsync(Guid id, CancellationToken ct = default)
        {
            var barbershop = await _repo.GetByIdAsync(id, ct)
                ?? throw NotFoundException.For("Barbershop", id);

            if (barbershop.IsMain && await _repo.HasActiveBranchesAsync(id, ct))
                throw new BusinessRuleException("No se puede eliminar una sede principal que tiene sucursales activas.");

            barbershop.Deactivate();
            _repo.Update(barbershop);
            await _uow.SaveChangesAsync(ct);
        }
    }
}
