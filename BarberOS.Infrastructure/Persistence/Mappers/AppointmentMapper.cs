using System.Reflection;
using BarberOS.Domain.Entities;
using BarberOS.Domain.Enums;
using BarberOS.Infrastructure.Persistence.DbModels;

namespace BarberOS.Infrastructure.Persistence.Mappers
{
    public static class AppointmentMapper
    {
        public static AppointmentDbModel ToDbModel(Appointment a) => new()
        {
            Id = a.Id,
            ClientId = a.ClientId,
            BarberId = a.BarberId,
            BarbershopId = a.BarbershopId,
            Date = a.Date,
            StartTime = a.StartTime,
            EndTime = a.EndTime,
            TotalPrice = a.TotalPrice,
            Status = (int)a.Status,
            Notes = a.Notes,
            CompletedAt = a.CompletedAt,
            CancelledAt = a.CancelledAt,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt,
            Services = a.Services.Select(s => new AppointmentServiceDbModel
            {
                Id = s.Id,
                AppointmentId = s.AppointmentId,
                ServiceId = s.ServiceId,
                ServiceName = s.ServiceName,
                Price = s.Price,
                DurationMinutes = s.DurationMinutes
            }).ToList()
        };

        public static Appointment ToDomain(AppointmentDbModel db)
        {
            var entity = (Appointment)Activator.CreateInstance(typeof(Appointment), nonPublic: true)!;
            var t = typeof(Appointment);

            SetPrivate(entity, t, nameof(Appointment.Id), db.Id);
            SetPrivate(entity, t, nameof(Appointment.ClientId), db.ClientId);
            SetPrivate(entity, t, nameof(Appointment.BarberId), db.BarberId);
            SetPrivate(entity, t, nameof(Appointment.BarbershopId), db.BarbershopId);
            SetPrivate(entity, t, nameof(Appointment.Date), db.Date);
            SetPrivate(entity, t, nameof(Appointment.StartTime), db.StartTime);
            SetPrivate(entity, t, nameof(Appointment.EndTime), db.EndTime);
            SetPrivate(entity, t, nameof(Appointment.TotalPrice), db.TotalPrice);
            SetPrivate(entity, t, nameof(Appointment.Status), (AppointmentStatus)db.Status);
            SetPrivate(entity, t, nameof(Appointment.Notes), db.Notes);
            SetPrivate(entity, t, nameof(Appointment.CompletedAt), db.CompletedAt);
            SetPrivate(entity, t, nameof(Appointment.CancelledAt), db.CancelledAt);
            SetPrivate(entity, t, nameof(Appointment.CreatedAt), db.CreatedAt);
            SetPrivate(entity, t, nameof(Appointment.UpdatedAt), db.UpdatedAt);

            var servicesField = t.GetField("_services", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var servicesList = (List<AppointmentService>)servicesField.GetValue(entity)!;

            foreach (var s in db.Services)
            {
                var svc = (AppointmentService)Activator.CreateInstance(typeof(AppointmentService), nonPublic: true)!;
                var st = typeof(AppointmentService);
                SetPrivate(svc, st, nameof(AppointmentService.Id), s.Id);
                SetPrivate(svc, st, nameof(AppointmentService.AppointmentId), s.AppointmentId);
                SetPrivate(svc, st, nameof(AppointmentService.ServiceId), s.ServiceId);
                SetPrivate(svc, st, nameof(AppointmentService.ServiceName), s.ServiceName);
                SetPrivate(svc, st, nameof(AppointmentService.Price), s.Price);
                SetPrivate(svc, st, nameof(AppointmentService.DurationMinutes), s.DurationMinutes);
                servicesList.Add(svc);
            }

            return entity;
        }

        private static void SetPrivate(object instance, Type type, string propertyName, object? value)
        {
            var prop = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public)!;
            prop.GetSetMethod(nonPublic: true)!.Invoke(instance, [value]);
        }
    }
}
