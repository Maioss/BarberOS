using BarberOS.Api.Common;
using BarberOS.Application.Metrics.DTOs;
using BarberOS.Application.Metrics.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberOS.Api.Controllers
{
    [ApiController]
    [Route("api/metrics")]
    [Authorize]
    public class MetricsController : ControllerBase
    {
        [HttpGet("barbershop/{id:guid}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> GetBarbershopMetrics(
            Guid id,
            [FromQuery] MetricsQuery query,
            [FromServices] GetBarbershopMetricsUseCase useCase,
            CancellationToken ct)
        {
            var result = await useCase.ExecuteAsync(id, query, ct);
            return Ok(ApiResponse<BarbershopMetricsDto>.Ok(result));
        }

        [HttpGet("barbers/me")]
        public async Task<IActionResult> GetMyMetrics(
            [FromQuery] MetricsQuery query,
            [FromServices] GetMyBarberMetricsUseCase useCase,
            CancellationToken ct)
        {
            var result = await useCase.ExecuteAsync(query, ct);
            return Ok(ApiResponse<BarberMetricsDto>.Ok(result));
        }
    }
}
