using System.Reflection;
using BarberOS.Application.Auth.UseCases;
using BarberOS.Application.Barbershops.UseCases;
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

            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
