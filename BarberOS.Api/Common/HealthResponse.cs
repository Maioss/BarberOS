using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace BarberOS.Api.Common
{
    public static class HealthResponse
    {
        public static Task Write(HttpContext context, HealthReport report)
        {
            context.Response.ContentType = "application/json";

            var payload = new
            {
                status = report.Status.ToString(),
                durationMs = (int)report.TotalDuration.TotalMilliseconds,
                checks = report.Entries.ToDictionary(
                    entry => entry.Key,
                    entry => entry.Value.Status.ToString())
            };

            var body = report.Status == HealthStatus.Healthy
                ? ApiResponse<object>.Ok(payload)
                : ApiResponse<object>.Fail(
                    "La aplicación no está saludable.",
                    report.Entries
                        .Where(entry => entry.Value.Status != HealthStatus.Healthy)
                        .Select(entry => entry.Key)
                        .ToList());

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            return context.Response.WriteAsync(JsonSerializer.Serialize(body, options));
        }
    }
}
