using BarberOS.Api.Common;
using Microsoft.AspNetCore.Mvc;

namespace BarberOS.Api.Controllers
{

    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            var payload = new
            {
                status = "OK",
                timestamp = DateTime.UtcNow,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
            };
            return Ok(ApiResponse<object>.Ok(payload));
        }
    }
}
