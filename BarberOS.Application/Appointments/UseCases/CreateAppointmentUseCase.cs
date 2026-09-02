using BarberOS.Application.Appointments.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Entities;
using BarberOS.Domain.Enums;
using BarberOS.Domain.Exceptions;

namespace BarberOS.Application.Appointments.UseCases
{
    public class CreateAppointmentUseCase
    {
        private readonly IAppointmentRepository _appointments;
        private readonly IBarberRepository _barbers;
        private readonly IBarbershopRepository _barbershops;
        private readonly IServiceRepository _services;
        private readonly IUserRepository _users;
        private readonly IBusinessClock _clock;
        private readonly ICurrentUserService _current;
        private readonly IUnitOfWork _uow;

        public CreateAppointmentUseCase(
            IAppointmentRepository appointments,
            IBarberRepository barbers,
            IBarbershopRepository barbershops,
            IServiceRepository services,
            IUserRepository users,
            IBusinessClock clock,
            ICurrentUserService current,
            IUnitOfWork uow)
        {
            _appointments = appointments;
            _barbers = barbers;
            _barbershops = barbershops;
            _services = services;
            _users = users;
            _clock = clock;
            _current = current;
            _uow = uow;
        }

        public async Task<AppointmentDto> ExecuteAsync(
            CreateAppointmentRequest request,
            CancellationToken ct = default)
        {
            var requestingUserId = _current.RequireUserId();
            var requestingRole = _current.RequireRole();

            var barber = await _barbers.GetByIdAsync(request.BarberId, ct)
                ?? throw NotFoundException.For("barbero", request.BarberId);

            if (!barber.IsActive)
                throw new BusinessRuleException("El barbero no está activo.");

            if (!barber.IsAvailableOn(request.Date.DayOfWeek))
                throw new BusinessRuleException("El barbero no trabaja ese día de la semana.");

            var barbershop = await _barbershops.GetByIdAsync(barber.BarbershopId, ct)
                ?? throw NotFoundException.For("barbería", barber.BarbershopId);

            if (!barbershop.IsActive)
                throw new BusinessRuleException("La barbería no está activa.");

            var today = _clock.Today(barbershop);
            if (request.Date < today)
                throw new BusinessRuleException("No se pueden crear reservas en fechas pasadas.");

            if (request.Date == today && request.StartTime <= _clock.TimeNow(barbershop))
                throw new BusinessRuleException("Ese horario ya pasó.");

            var serviceList = await _services.GetManyByIdsAsync(request.ServiceIds, ct);
            if (serviceList.Count != request.ServiceIds.Distinct().Count())
                throw new BusinessRuleException("Uno o más servicios no fueron encontrados.");

            foreach (var svc in serviceList)
            {
                if (!svc.IsActive)
                    throw new BusinessRuleException($"El servicio '{svc.Name}' no está activo.");
            }

            var principalId = barbershop.IsMain ? barbershop.Id : barbershop.ParentId!.Value;
            if (serviceList.Any(s => s.BarbershopId != principalId))
                throw new BusinessRuleException("Los servicios no pertenecen a la misma barbería del barbero.");

            var totalMinutes = serviceList.Sum(s => s.DurationMinutes);
            var endTime = request.StartTime.AddMinutes(totalMinutes);

            var workdayStart = new TimeOnly(9, 0);
            var workdayEnd = new TimeOnly(18, 0);
            if (request.StartTime < workdayStart || endTime > workdayEnd)
                throw new BusinessRuleException("La reserva está fuera del horario de atención (09:00–18:00).");

            var lunchStart = barber.LunchStart;
            var lunchEnd = barber.LunchEnd;
            if (request.StartTime < lunchEnd && endTime > lunchStart)
                throw new BusinessRuleException("La reserva se superpone con el horario de almuerzo del barbero.");

            var existing = await _appointments.ListByBarberAndDateAsync(
                barber.Id, request.Date, AppointmentStatus.Confirmed, ct);

            var proposed = new { Start = request.StartTime, End = endTime };
            var conflict = existing.Any(a => a.StartTime < proposed.End && proposed.Start < a.EndTime);
            if (conflict)
                throw new BusinessRuleException("El barbero ya tiene una reserva confirmada en ese horario.");

            var clientId = requestingUserId;
            if (request.ClientId is not null && request.ClientId.Value != requestingUserId)
            {
                if (requestingRole is not (Role.Admin or Role.SuperAdmin))
                    throw new ForbiddenException("No puedes reservar a nombre de otro usuario.");

                var target = await _users.GetByIdAsync(request.ClientId.Value, ct)
                    ?? throw NotFoundException.For("cliente", request.ClientId.Value);

                if (target.Role != Role.Client)
                    throw new BusinessRuleException("Solo se puede reservar a nombre de un cliente.");

                if (!target.IsActive)
                    throw new BusinessRuleException("El cliente está desactivado.");

                clientId = target.Id;
            }

            var clientConflict = await _appointments.ClientHasConflictingAppointmentAsync(
                clientId, request.Date, request.StartTime, endTime, ct);
            if (clientConflict)
                throw new ConflictException("El cliente ya tiene otra reserva en ese horario.");

            var appointment = Appointment.Create(
                clientId,
                barber.Id,
                barber.BarbershopId,
                request.Date,
                request.StartTime,
                serviceList,
                request.Notes);

            await _appointments.AddAsync(appointment, ct);
            await _uow.SaveChangesAsync(ct);

            var clientUser = await _users.GetByIdAsync(clientId, ct);
            var barberUser = await _users.GetByIdAsync(barber.UserId, ct);
            return MapToDto(appointment, clientUser?.FullName ?? string.Empty, barberUser?.FullName ?? string.Empty);
        }

        internal static AppointmentDto MapToDto(Appointment a, string clientName, string barberName) => new(
            a.Id,
            a.ClientId,
            clientName,
            a.BarberId,
            barberName,
            a.BarbershopId,
            a.Date,
            a.StartTime,
            a.EndTime,
            a.TotalPrice,
            a.Status,
            a.Notes,
            a.CompletedAt,
            a.CancelledAt,
            a.CreatedAt,
            a.Services.Select(s => new AppointmentServiceDto(s.ServiceId, s.ServiceName, s.Price, s.DurationMinutes)).ToList());

        /// <summary>
        /// Resuelve los nombres en dos consultas para toda la lista. Uno por uno eran
        /// tres por cita: 61 consultas para devolver una página de 19.
        /// </summary>
        internal static async Task<List<AppointmentDto>> MapManyAsync(
            IReadOnlyList<Appointment> appointments,
            IUserRepository users,
            IBarberRepository barbers,
            CancellationToken ct)
        {
            if (appointments.Count == 0) return [];

            var barbersById = (await barbers.GetManyByIdsAsync(appointments.Select(a => a.BarberId), ct))
                .ToDictionary(b => b.Id);

            var userIds = appointments.Select(a => a.ClientId)
                .Concat(barbersById.Values.Select(b => b.UserId));
            var namesByUserId = (await users.GetManyByIdsAsync(userIds, ct))
                .ToDictionary(u => u.Id, u => u.FullName);

            return appointments.Select(a =>
            {
                var barberUserId = barbersById.TryGetValue(a.BarberId, out var barber) ? barber.UserId : Guid.Empty;
                return MapToDto(
                    a,
                    namesByUserId.GetValueOrDefault(a.ClientId, string.Empty),
                    namesByUserId.GetValueOrDefault(barberUserId, string.Empty));
            }).ToList();
        }

        internal static async Task<AppointmentDto> MapToDtoAsync(
            Appointment a,
            IUserRepository users,
            IBarberRepository barbers,
            CancellationToken ct) =>
            (await MapManyAsync([a], users, barbers, ct))[0];
    }
}
