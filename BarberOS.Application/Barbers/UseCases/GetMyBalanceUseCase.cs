using BarberOS.Application.Barbers.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Enums;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Barbers.UseCases
{
    public class GetMyBalanceUseCase
    {
        private readonly IBarberRepository _barbers;
        private readonly IBalanceEntryRepository _ledger;
        private readonly ICurrentUserService _current;

        public GetMyBalanceUseCase(
            IBarberRepository barbers,
            IBalanceEntryRepository ledger,
            ICurrentUserService current)
        {
            _barbers = barbers;
            _ledger = ledger;
            _current = current;
        }

        public async Task<BalanceDto> ExecuteAsync(CancellationToken ct = default)
        {
            if (!_current.IsAuthenticated || _current.UserId is null)
                throw new UnauthorizedException("No autenticado.");

            if (_current.Role != Role.Barber)
                throw new ForbiddenException("Solo barberos pueden consultar su saldo.");

            var barber = await _barbers.GetByUserIdAsync(_current.UserId.Value, ct)
                ?? throw new NotFoundException("No tienes un perfil de barbero registrado.");

            var balance = await _ledger.GetBalanceAsync(barber.Id, ct);

            return new BalanceDto(barber.Id, balance);
        }
    }
}
