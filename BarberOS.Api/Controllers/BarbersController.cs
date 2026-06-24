using BarberOS.Api.Common;
using BarberOS.Application.Barbers.DTOs;
using BarberOS.Application.Barbers.UseCases;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberOS.Api.Controllers
{
    [ApiController]
    [Route("api/barbers")]
    public class BarbersController : ControllerBase
    {
        [HttpGet("me/balance")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<BalanceDto>>> GetMyBalance(
            [FromServices] GetMyBalanceUseCase useCase,
            CancellationToken ct)
        {
            var result = await useCase.ExecuteAsync(ct);
            return Ok(ApiResponse<BalanceDto>.Ok(result));
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<BarberDto>>> GetById(
            Guid id,
            [FromServices] GetBarberByIdUseCase useCase,
            CancellationToken ct)
        {
            var result = await useCase.ExecuteAsync(id, ct);
            return Ok(ApiResponse<BarberDto>.Ok(result));
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<ApiResponse<BarberDto>>> Create(
            [FromBody] CreateBarberRequest request,
            [FromServices] IValidator<CreateBarberRequest> validator,
            [FromServices] CreateBarberUseCase useCase,
            CancellationToken ct)
        {
            await validator.ValidateAndThrowAsync(request, ct);
            var result = await useCase.ExecuteAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<BarberDto>.Ok(result, "Perfil de barbero creado."));
        }

        [HttpPut("{id:guid}/schedule")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<BarberDto>>> UpdateSchedule(
            Guid id,
            [FromBody] UpdateScheduleRequest request,
            [FromServices] IValidator<UpdateScheduleRequest> validator,
            [FromServices] UpdateScheduleUseCase useCase,
            CancellationToken ct)
        {
            await validator.ValidateAndThrowAsync(request, ct);
            var result = await useCase.ExecuteAsync(id, request, ct);
            return Ok(ApiResponse<BarberDto>.Ok(result, "Horario actualizado."));
        }

        [HttpGet("{id:guid}/availability")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<AvailabilityDto>>> GetAvailability(
            Guid id,
            [FromQuery] DateOnly date,
            [FromServices] GetAvailabilityUseCase useCase,
            CancellationToken ct)
        {
            var result = await useCase.ExecuteAsync(id, date, ct);
            return Ok(ApiResponse<AvailabilityDto>.Ok(result));
        }
    }
}
