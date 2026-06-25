using BarberOS.Application.Appointments.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Entities;
using BarberOS.Domain.Enums;
using BarberOS.Infrastructure.Persistence.DbModels;
using BarberOS.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace BarberOS.Infrastructure.Persistence.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly BarberOSDbContext _db;

        public AppointmentRepository(BarberOSDbContext db) => _db = db;

        public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var db = await _db.Appointments
                .Include(a => a.Services)
                .FirstOrDefaultAsync(a => a.Id == id, ct);
            return db is null ? null : AppointmentMapper.ToDomain(db);
        }

        public async Task<IReadOnlyList<Appointment>> ListByBarberAndDateAsync(
            Guid barberId, DateOnly date, AppointmentStatus status, CancellationToken ct = default)
        {
            var statusInt = (int)status;
            var list = await _db.Appointments
                .Include(a => a.Services)
                .Where(a => a.BarberId == barberId && a.Date == date && a.Status == statusInt)
                .ToListAsync(ct);
            return list.Select(AppointmentMapper.ToDomain).ToList();
        }

        public async Task<PagedResult<Appointment>> ListAsync(AppointmentFilter filter, CancellationToken ct = default)
        {
            var query = _db.Appointments.Include(a => a.Services).AsQueryable();

            if (filter.BarberId.HasValue)
                query = query.Where(a => a.BarberId == filter.BarberId.Value);

            if (filter.BarbershopId.HasValue)
                query = query.Where(a => a.BarbershopId == filter.BarbershopId.Value);

            if (filter.ClientId.HasValue)
                query = query.Where(a => a.ClientId == filter.ClientId.Value);

            if (filter.Status.HasValue)
                query = query.Where(a => a.Status == (int)filter.Status.Value);

            if (filter.DateFrom.HasValue)
                query = query.Where(a => a.Date >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                query = query.Where(a => a.Date <= filter.DateTo.Value);

            var total = await query.CountAsync(ct);
            var page = filter.Page < 1 ? 1 : filter.Page;
            var pageSize = filter.PageSize < 1 ? 10 : filter.PageSize;

            var items = await query
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.StartTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResult<Appointment>(
                items.Select(AppointmentMapper.ToDomain).ToList(),
                page, pageSize, total);
        }

        public async Task<PagedResult<Appointment>> ListByClientAsync(
            Guid clientId, AppointmentFilter filter, CancellationToken ct = default)
        {
            return await ListAsync(filter with { ClientId = clientId }, ct);
        }

        public async Task<bool> ClientHasConflictingAppointmentAsync(
            Guid clientId, DateOnly date, TimeOnly start, TimeOnly end, CancellationToken ct = default)
        {
            var confirmed = (int)AppointmentStatus.Confirmed;
            return await _db.Appointments.AnyAsync(x =>
                x.ClientId == clientId &&
                x.Date == date &&
                x.Status == confirmed &&
                x.StartTime < end &&
                start < x.EndTime, ct);
        }

        public async Task AddAsync(Appointment appointment, CancellationToken ct = default)
        {
            var db = AppointmentMapper.ToDbModel(appointment);
            await _db.Appointments.AddAsync(db, ct);
        }

        public void Update(Appointment appointment)
        {
            var existing = _db.Appointments.Local.FirstOrDefault(a => a.Id == appointment.Id);
            if (existing is not null)
            {
                _db.Entry(existing).CurrentValues.SetValues(AppointmentMapper.ToDbModel(appointment));
            }
            else
            {
                var db = AppointmentMapper.ToDbModel(appointment);
                _db.Entry(db).State = EntityState.Modified;
            }
        }
    }
}
