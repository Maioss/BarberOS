using BarberOS.Application.Metrics.DTOs;
using BarberOS.Application.Shared;
using BarberOS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BarberOS.Infrastructure.Persistence.Repositories
{
    public class MetricsRepository : IMetricsRepository
    {
        private readonly BarberOSDbContext _db;

        public MetricsRepository(BarberOSDbContext db) => _db = db;

        public async Task<BarbershopMetricsDto?> GetBarbershopMetricsAsync(
            Guid principalBarbershopId, DateOnly from, DateOnly to, CancellationToken ct = default)
        {
            var principal = await _db.Barbershops.AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == principalBarbershopId && b.IsMain, ct);
            if (principal is null) return null;

            var siteIds = await _db.Barbershops.AsNoTracking()
                .Where(b => b.Id == principalBarbershopId || b.ParentId == principalBarbershopId)
                .Select(b => b.Id)
                .ToListAsync(ct);

            var completedStatus = (int)AppointmentStatus.Completed;
            var cancelledStatus = (int)AppointmentStatus.Cancelled;

            var dateFrom = from;
            var dateTo = to;
            var fromDt = DateTime.SpecifyKind(dateFrom.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            var toDt = DateTime.SpecifyKind(dateTo.AddDays(1).ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);

            var totalAppointments = await _db.Appointments.AsNoTracking()
                .CountAsync(a => siteIds.Contains(a.BarbershopId) && a.Date >= dateFrom && a.Date <= dateTo, ct);

            var completedAppointments = await _db.Appointments.AsNoTracking()
                .CountAsync(a => siteIds.Contains(a.BarbershopId) && a.Date >= dateFrom && a.Date <= dateTo
                                 && a.Status == completedStatus, ct);

            var cancelledAppointments = await _db.Appointments.AsNoTracking()
                .CountAsync(a => siteIds.Contains(a.BarbershopId) && a.Date >= dateFrom && a.Date <= dateTo
                                 && a.Status == cancelledStatus, ct);

            var completionRate = totalAppointments == 0
                ? 0m
                : Math.Round((decimal)completedAppointments / totalAppointments, 2);

            var grossRevenue = await _db.Appointments.AsNoTracking()
                .Where(a => siteIds.Contains(a.BarbershopId) && a.Date >= dateFrom && a.Date <= dateTo
                            && a.Status == completedStatus)
                .SumAsync(a => (decimal?)a.TotalPrice, ct) ?? 0m;

            // Refunds: join payments + appointments, materialize amounts, sum in memory
            var appointmentIdsInScope = await _db.Appointments.AsNoTracking()
                .Where(a => siteIds.Contains(a.BarbershopId) && a.Date >= dateFrom && a.Date <= dateTo)
                .Select(a => a.Id)
                .ToListAsync(ct);

            var refunds = appointmentIdsInScope.Count == 0 ? 0m
                : await _db.Payments.AsNoTracking()
                    .Where(p => appointmentIdsInScope.Contains(p.AppointmentId)
                                && p.Status == (int)PaymentStatus.Refunded
                                && p.CreatedAt >= fromDt && p.CreatedAt < toDt)
                    .SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;

            var netRevenue = grossRevenue - refunds;

            // PaymentsByMethod: fetch scalar rows, group in memory
            var paidPaymentRows = appointmentIdsInScope.Count == 0
                ? new[] { new { Method = 0, Amount = 0m } }.Take(0).ToList()
                : await _db.Payments.AsNoTracking()
                    .Where(p => appointmentIdsInScope.Contains(p.AppointmentId)
                                && p.Status == (int)PaymentStatus.Paid
                                && p.CreatedAt >= fromDt && p.CreatedAt < toDt)
                    .Select(p => new { p.Method, p.Amount })
                    .ToListAsync(ct);

            var paymentsByMethod = paidPaymentRows
                .GroupBy(p => ((PaymentMethod)p.Method).ToString())
                .Where(g => g.Sum(p => p.Amount) > 0)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.Amount));

            // TopServices: fetch appointment+service rows, group in memory
            var completedApptIds = await _db.Appointments.AsNoTracking()
                .Where(a => siteIds.Contains(a.BarbershopId) && a.Date >= dateFrom && a.Date <= dateTo
                            && a.Status == completedStatus)
                .Select(a => a.Id)
                .ToListAsync(ct);

            var serviceRows = completedApptIds.Count == 0
                ? new[] { new { ServiceId = Guid.Empty, ServiceName = string.Empty, Price = 0m } }.Take(0).ToList()
                : await _db.AppointmentServices.AsNoTracking()
                    .Where(s => completedApptIds.Contains(s.AppointmentId))
                    .Select(s => new { s.ServiceId, s.ServiceName, s.Price })
                    .ToListAsync(ct);

            var topServices = serviceRows
                .GroupBy(s => new { s.ServiceId, s.ServiceName })
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new TopServiceDto(g.Key.ServiceId, g.Key.ServiceName, g.Count(), g.Sum(s => s.Price)))
                .ToList();

            // TopBarbers: fetch appointment+barber+user rows, group in memory
            var barberApptRows = await (
                from a in _db.Appointments.AsNoTracking()
                join b in _db.Barbers.AsNoTracking() on a.BarberId equals b.Id
                join u in _db.Users.AsNoTracking() on b.UserId equals u.Id
                where siteIds.Contains(a.BarbershopId)
                      && a.Date >= dateFrom && a.Date <= dateTo
                      && a.Status == completedStatus
                select new { BarberId = a.BarberId, BarberName = u.FullName, Revenue = a.TotalPrice }
            ).ToListAsync(ct);

            var topBarbers = barberApptRows
                .GroupBy(x => new { x.BarberId, x.BarberName })
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new TopBarberDto(g.Key.BarberId, g.Key.BarberName, g.Count(), g.Sum(x => x.Revenue)))
                .ToList();

            return new BarbershopMetricsDto(
                principal.Id,
                principal.Name,
                dateFrom,
                dateTo,
                totalAppointments,
                completedAppointments,
                cancelledAppointments,
                completionRate,
                grossRevenue,
                refunds,
                netRevenue,
                paymentsByMethod,
                topServices,
                topBarbers);
        }

        public async Task<BarberMetricsDto?> GetBarberMetricsAsync(
            Guid barberId, DateOnly from, DateOnly to, CancellationToken ct = default)
        {
            var barber = await _db.Barbers.AsNoTracking().FirstOrDefaultAsync(b => b.Id == barberId, ct);
            if (barber is null) return null;

            var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == barber.UserId, ct);
            if (user is null) return null;

            var completedStatus = (int)AppointmentStatus.Completed;
            var cancelledStatus = (int)AppointmentStatus.Cancelled;

            var dateFrom = from;
            var dateTo = to;

            var totalAppointments = await _db.Appointments.AsNoTracking()
                .CountAsync(a => a.BarberId == barberId && a.Date >= dateFrom && a.Date <= dateTo, ct);

            var completedAppointments = await _db.Appointments.AsNoTracking()
                .CountAsync(a => a.BarberId == barberId && a.Date >= dateFrom && a.Date <= dateTo
                                 && a.Status == completedStatus, ct);

            var cancelledAppointments = await _db.Appointments.AsNoTracking()
                .CountAsync(a => a.BarberId == barberId && a.Date >= dateFrom && a.Date <= dateTo
                                 && a.Status == cancelledStatus, ct);

            var completionRate = totalAppointments == 0
                ? 0m
                : Math.Round((decimal)completedAppointments / totalAppointments, 2);

            var grossRevenue = await _db.Appointments.AsNoTracking()
                .Where(a => a.BarberId == barberId && a.Date >= dateFrom && a.Date <= dateTo
                            && a.Status == completedStatus)
                .SumAsync(a => (decimal?)a.TotalPrice, ct) ?? 0m;

            var currentBalance = await _db.BalanceEntries.AsNoTracking()
                .Where(e => e.BarberId == barberId)
                .SumAsync(e => (decimal?)e.Amount, ct) ?? 0m;

            // TopServices: fetch rows, group in memory
            var completedApptIds = await _db.Appointments.AsNoTracking()
                .Where(a => a.BarberId == barberId && a.Date >= dateFrom && a.Date <= dateTo
                            && a.Status == completedStatus)
                .Select(a => a.Id)
                .ToListAsync(ct);

            var serviceRows = completedApptIds.Count == 0
                ? new[] { new { ServiceId = Guid.Empty, ServiceName = string.Empty, Price = 0m } }.Take(0).ToList()
                : await _db.AppointmentServices.AsNoTracking()
                    .Where(s => completedApptIds.Contains(s.AppointmentId))
                    .Select(s => new { s.ServiceId, s.ServiceName, s.Price })
                    .ToListAsync(ct);

            var topServices = serviceRows
                .GroupBy(s => new { s.ServiceId, s.ServiceName })
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => new TopServiceDto(g.Key.ServiceId, g.Key.ServiceName, g.Count(), g.Sum(s => s.Price)))
                .ToList();

            return new BarberMetricsDto(
                barber.Id,
                user.FullName,
                barber.BarbershopId,
                dateFrom,
                dateTo,
                totalAppointments,
                completedAppointments,
                cancelledAppointments,
                completionRate,
                grossRevenue,
                currentBalance,
                topServices);
        }
    }
}
