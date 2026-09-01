using System.Globalization;
using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace BarberOS.Api.Common
{
    public static class AuthRateLimit
    {
        public const string PolicyName = "auth";

        private const int PermitLimit = 10;
        private static readonly TimeSpan Window = TimeSpan.FromMinutes(1);

        public static RateLimitPartition<string> Partition(HttpContext context) =>
            RateLimitPartition.GetFixedWindowLimiter(
                context.Connection.RemoteIpAddress?.ToString() ?? "sin-ip",
                _ => new FixedWindowRateLimiterOptions { PermitLimit = PermitLimit, Window = Window });

        public static ValueTask WriteRejection(OnRejectedContext context, CancellationToken ct)
        {
            var response = context.HttpContext.Response;
            response.StatusCode = StatusCodes.Status429TooManyRequests;
            response.ContentType = "application/json";

            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);

            var payload = ApiResponse<object>.Fail("Demasiados intentos. Espera un momento y vuelve a probar.");
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            return new ValueTask(response.WriteAsync(JsonSerializer.Serialize(payload, options), ct));
        }
    }
}
