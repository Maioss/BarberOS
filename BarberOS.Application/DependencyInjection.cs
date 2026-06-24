using System.Reflection;
using BarberOS.Application.Auth.UseCases;
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

            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
