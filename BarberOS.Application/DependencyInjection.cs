using System.Reflection;
using BarberOS.Application.Auth.UseCases;
using BarberOS.Application.Barbershops.UseCases;
using BarberOS.Application.Users.UseCases;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BarberOS.Application
{

    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<LoginUseCase>();
            services.AddScoped<RegisterClientUseCase>();
            services.AddScoped<GetCurrentUserUseCase>();

            services.AddScoped<ListBarbershopsUseCase>();
            services.AddScoped<GetBarbershopByIdUseCase>();
            services.AddScoped<ListBranchesUseCase>();
            services.AddScoped<CreateBarbershopUseCase>();
            services.AddScoped<UpdateBarbershopUseCase>();
            services.AddScoped<DeleteBarbershopUseCase>();

            services.AddScoped<GetUserByIdUseCase>();
            services.AddScoped<ListUsersUseCase>();
            services.AddScoped<CreateUserUseCase>();
            services.AddScoped<UpdateUserUseCase>();
            services.AddScoped<DeleteUserUseCase>();

            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
