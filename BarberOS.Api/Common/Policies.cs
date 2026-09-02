using BarberOS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace BarberOS.Api.Common
{
    public static class Policies
    {
        /// <summary>Crear, editar y desactivar barberías.</summary>
        public const string OwnerOnly = "owner";
        public const string Management = "management";
        public const string ClientOnly = "client";
        public const string BarberOnly = "barber";
        public const string CanBook = "can-book";
        public const string CanCancel = "can-cancel";
        public const string CanComplete = "can-complete";

        public static void AddBarberOSPolicies(this AuthorizationOptions options)
        {
            // Un endpoint sin atributo queda protegido. Antes, olvidar el rol en un
            // verbo de escritura lo dejaba abierto a cualquier autenticado.
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            options.AddPolicy(OwnerOnly, p => p.RequireRole(Named(Role.SuperAdmin)));
            options.AddPolicy(Management, p => p.RequireRole(Named(Role.SuperAdmin), Named(Role.Admin)));
            options.AddPolicy(ClientOnly, p => p.RequireRole(Named(Role.Client)));
            options.AddPolicy(BarberOnly, p => p.RequireRole(Named(Role.Barber)));

            options.AddPolicy(CanBook, p => p.RequireRole(
                Named(Role.SuperAdmin), Named(Role.Admin), Named(Role.Client)));

            options.AddPolicy(CanCancel, p => p.RequireRole(
                Named(Role.SuperAdmin), Named(Role.Admin), Named(Role.Client), Named(Role.Barber)));

            options.AddPolicy(CanComplete, p => p.RequireRole(
                Named(Role.SuperAdmin), Named(Role.Admin), Named(Role.Barber)));
        }

        /// <summary>Ata el nombre del rol al enum: renombrarlo rompe la compilación, no la seguridad.</summary>
        private static string Named(Role role) => role.ToString();
    }
}
