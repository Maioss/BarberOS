using System.Text;
using BarberOS.Application.Shared;
using BarberOS.Infrastructure.Persistence;
using BarberOS.Infrastructure.Persistence.Repositories;
using BarberOS.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace BarberOS.Infrastructure
{

    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Default")
                ?? throw new InvalidOperationException("Falta la cadena de conexión 'Default'.");

            services.AddDbContext<BarberOSDbContext>(options => options.UseNpgsql(connectionString));
            services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<BarberOSDbContext>());

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IBarbershopRepository, BarbershopRepository>();
            services.AddScoped<IBarberRepository, BarberRepository>();
            services.AddScoped<IServiceRepository, ServiceRepository>();
            services.AddScoped<IAppointmentRepository, AppointmentRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IBalanceEntryRepository, BalanceEntryRepository>();
            services.AddScoped<IMetricsRepository, MetricsRepository>();
            services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();

            services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            var jwt = configuration.GetSection("Jwt").Get<JwtSettings>()
                ?? throw new InvalidOperationException("Falta la sección 'Jwt' en configuración.");

            if (string.IsNullOrWhiteSpace(jwt.Secret) || Encoding.UTF8.GetByteCount(jwt.Secret) < 32)
                throw new InvalidOperationException(
                    "Falta 'Jwt:Secret' o es mas corto que 32 bytes. " +
                    "En desarrollo: dotnet user-secrets set \"Jwt:Secret\" \"<valor>\" --project BarberOS.Api. " +
                    "En produccion: variable de entorno Jwt__Secret.");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwt.Issuer,
                        ValidAudience = jwt.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
                        ClockSkew = TimeSpan.FromSeconds(30)
                    };
                });

            services.AddAuthorization();

            return services;
        }
    }
}
