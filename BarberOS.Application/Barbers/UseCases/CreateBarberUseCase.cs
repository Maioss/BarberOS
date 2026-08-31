using BarberOS.Application.Barbers.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Entities;
using BarberOS.Domain.Enums;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Barbers.UseCases
{
    public class CreateBarberUseCase
    {
        private readonly IBarberRepository _barbers;
        private readonly IUserRepository _users;
        private readonly TenantScope _scope;
        private readonly IUnitOfWork _uow;

        public CreateBarberUseCase(IBarberRepository barbers, IUserRepository users, TenantScope scope, IUnitOfWork uow)
        {
            _barbers = barbers;
            _users = users;
            _scope = scope;
            _uow = uow;
        }

        public async Task<BarberDto> ExecuteAsync(CreateBarberRequest request, CancellationToken ct = default)
        {
            var user = await _users.GetByIdAsync(request.UserId, ct)
                ?? throw NotFoundException.For("usuario", request.UserId);

            if (user.Role != Role.Barber)
                throw new BusinessRuleException("Solo usuarios con rol Barber pueden tener perfil de barbero.");

            if (!user.IsActive)
                throw new BusinessRuleException("El usuario está desactivado.");

            if (user.BarbershopId is null)
                throw new BusinessRuleException("El usuario Barber no tiene barbería asociada.");

            await _scope.EnsureInScopeAsync(user.BarbershopId.Value, ct);

            if (await _barbers.ExistsByUserIdAsync(request.UserId, ct))
                throw new ConflictException("Ya existe un perfil de barbero para este usuario.");

            var barber = Barber.Create(user.Id, user.BarbershopId.Value);
            await _barbers.AddAsync(barber, ct);
            await _uow.SaveChangesAsync(ct);

            return new BarberDto(
                barber.Id, user.Id, user.FullName, user.Phone, barber.BarbershopId,
                barber.LunchStart, barber.LunchEnd, barber.GetAvailableDays(), barber.IsActive
            );
        }
    }
}
