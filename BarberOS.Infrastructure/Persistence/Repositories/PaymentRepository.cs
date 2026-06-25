using BarberOS.Application.Payments.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Entities;
using BarberOS.Infrastructure.Persistence.Mappers;
using Microsoft.EntityFrameworkCore;

namespace BarberOS.Infrastructure.Persistence.Repositories
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly BarberOSDbContext _db;

        public PaymentRepository(BarberOSDbContext db) => _db = db;

        public async Task<Payment?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var db = await _db.Payments.FirstOrDefaultAsync(p => p.Id == id, ct);
            return db is null ? null : PaymentMapper.ToDomain(db);
        }

        public async Task<Payment?> GetByAppointmentIdAsync(Guid appointmentId, CancellationToken ct = default)
        {
            var db = await _db.Payments.FirstOrDefaultAsync(p => p.AppointmentId == appointmentId, ct);
            return db is null ? null : PaymentMapper.ToDomain(db);
        }

        public async Task<PagedResult<Payment>> ListAsync(PaymentFilter filter, CancellationToken ct = default)
        {
            var query = _db.Payments.AsQueryable();

            if (filter.AppointmentId.HasValue)
                query = query.Where(p => p.AppointmentId == filter.AppointmentId.Value);

            if (filter.ClientId.HasValue)
                query = query.Where(p => p.ClientId == filter.ClientId.Value);

            if (filter.BarberId.HasValue)
                query = query.Where(p => p.BarberId == filter.BarberId.Value);

            if (filter.BarbershopId.HasValue)
                query = query.Where(p => p.BarbershopId == filter.BarbershopId.Value);

            if (filter.Status.HasValue)
                query = query.Where(p => p.Status == (int)filter.Status.Value);

            if (filter.DateFrom.HasValue)
            {
                var from = filter.DateFrom.Value.ToDateTime(TimeOnly.MinValue);
                query = query.Where(p => p.CreatedAt >= from);
            }

            if (filter.DateTo.HasValue)
            {
                var to = filter.DateTo.Value.ToDateTime(TimeOnly.MaxValue);
                query = query.Where(p => p.CreatedAt <= to);
            }

            var total = await query.CountAsync(ct);
            var page = filter.Page < 1 ? 1 : filter.Page;
            var pageSize = filter.PageSize < 1 ? 10 : filter.PageSize;

            var items = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PagedResult<Payment>(
                items.Select(PaymentMapper.ToDomain).ToList(),
                page, pageSize, total);
        }

        public async Task AddAsync(Payment payment, CancellationToken ct = default)
        {
            var db = PaymentMapper.ToDbModel(payment);
            await _db.Payments.AddAsync(db, ct);
        }

        public void Update(Payment payment)
        {
            var existing = _db.Payments.Local.FirstOrDefault(p => p.Id == payment.Id);
            if (existing is not null)
            {
                _db.Entry(existing).CurrentValues.SetValues(PaymentMapper.ToDbModel(payment));
            }
            else
            {
                var db = PaymentMapper.ToDbModel(payment);
                _db.Entry(db).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            }
        }
    }
}
