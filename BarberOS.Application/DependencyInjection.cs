using System.Reflection;
using BarberOS.Application.Shared;
using BarberOS.Application.Auth;
using BarberOS.Application.Auth.UseCases;
using BarberOS.Application.Barbershops.UseCases;
using BarberOS.Application.Barbers.UseCases;
using BarberOS.Application.Appointments.UseCases;
using BarberOS.Application.Metrics.UseCases;
using BarberOS.Application.Payments.UseCases;
using BarberOS.Application.Services.UseCases;
using BarberOS.Application.Users.UseCases;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BarberOS.Application
{

    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<TenantScope>();
            services.AddScoped<LoginUseCase>();
            services.AddScoped<RegisterClientUseCase>();
            services.AddScoped<GetCurrentUserUseCase>();
            services.AddScoped<RefreshSessionUseCase>();
            services.AddScoped<LogoutUseCase>();
            services.AddScoped<SessionIssuer>();

            services.AddScoped<ListBarbershopsUseCase>();
            services.AddScoped<GetBarbershopByIdUseCase>();
            services.AddScoped<ListBranchesUseCase>();
            services.AddScoped<CreateBarbershopUseCase>();
            services.AddScoped<UpdateBarbershopUseCase>();
            services.AddScoped<DeleteBarbershopUseCase>();

            services.AddScoped<GetUserByIdUseCase>();
            services.AddScoped<GetMyProfileUseCase>();
            services.AddScoped<ListUsersUseCase>();
            services.AddScoped<CreateUserUseCase>();
            services.AddScoped<UpdateUserUseCase>();
            services.AddScoped<UpdateMyProfileUseCase>();
            services.AddScoped<UpdateMyPhotoUseCase>();
            services.AddScoped<DeleteUserUseCase>();

            services.AddScoped<CreateBarberUseCase>();
            services.AddScoped<OnboardBarberUseCase>();
            services.AddScoped<GetBarberByIdUseCase>();
            services.AddScoped<ListBarbersByBarbershopUseCase>();
            services.AddScoped<ListBarbersForAdminUseCase>();
            services.AddScoped<UpdateScheduleUseCase>();
            services.AddScoped<GetAvailabilityUseCase>();
            services.AddScoped<GetMyBalanceUseCase>();

            services.AddScoped<ListServicesByBarbershopUseCase>();
            services.AddScoped<CreateServiceUseCase>();
            services.AddScoped<UpdateServiceUseCase>();
            services.AddScoped<DeleteServiceUseCase>();

            services.AddScoped<CreateAppointmentUseCase>();
            services.AddScoped<GetAppointmentByIdUseCase>();
            services.AddScoped<ListMyAppointmentsUseCase>();
            services.AddScoped<ListAppointmentsUseCase>();
            services.AddScoped<CancelAppointmentUseCase>();
            services.AddScoped<CompleteAppointmentUseCase>();
            services.AddScoped<ListBarberScheduleUseCase>();

            services.AddScoped<RegisterPaymentUseCase>();
            services.AddScoped<GetPaymentByIdUseCase>();
            services.AddScoped<ListPaymentsUseCase>();
            services.AddScoped<ListMyPaymentsUseCase>();
            services.AddScoped<RefundPaymentUseCase>();

            services.AddScoped<GetBarbershopMetricsUseCase>();
            services.AddScoped<GetMyBarberMetricsUseCase>();

            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
