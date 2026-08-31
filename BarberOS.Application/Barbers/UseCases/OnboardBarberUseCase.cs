using BarberOS.Application.Barbers.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Entities;
using BarberOS.Domain.Enums;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Barbers.UseCases
{
    public class OnboardBarberUseCase
    {
        private readonly IUserRepository _users;
        private readonly IBarberRepository _barbers;
        private readonly IBarbershopRepository _shops;
        private readonly IPasswordHasher _hasher;
        private readonly TenantScope _scope;
        private readonly IUnitOfWork _uow;

        public OnboardBarberUseCase(
            IUserRepository users,
            IBarberRepository barbers,
            IBarbershopRepository shops,
            IPasswordHasher hasher,
            TenantScope scope,
            IUnitOfWork uow)
        {
            _users = users;
            _barbers = barbers;
            _shops = shops;
            _hasher = hasher;
            _scope = scope;
            _uow = uow;
        }

        public async Task<BarberDto> ExecuteAsync(OnboardBarberRequest request, CancellationToken ct = default)
        {
            var shop = await _shops.GetByIdAsync(request.BarbershopId, ct)
                ?? throw NotFoundException.For("barbería", request.BarbershopId);

            await _scope.EnsureInScopeAsync(shop.Id, ct);

            if (!shop.IsActive)
                throw new BusinessRuleException("No se puede asociar un barbero a una barbería inactiva.");

            if (await _users.ExistsByEmailAsync(request.Email, ct))
                throw new ConflictException("Ya existe una cuenta con ese correo.");

            var user = User.Create(
                request.Email,
                _hasher.Hash(request.Password),
                request.FullName,
                Role.Barber,
                request.Phone,
                shop.Id);

            var barber = Barber.Create(user.Id, shop.Id);

            await _users.AddAsync(user, ct);
            await _barbers.AddAsync(barber, ct);

            // Un solo SaveChanges: o entran los dos o no entra ninguno.
            await _uow.SaveChangesAsync(ct);

            return new BarberDto(
                barber.Id, user.Id, user.FullName, user.Phone, barber.BarbershopId,
                barber.LunchStart, barber.LunchEnd, barber.GetAvailableDays(), barber.IsActive);
        }
    }
}
