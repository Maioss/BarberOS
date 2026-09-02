using BarberOS.Api.Common;
using BarberOS.Application.Appointments.DTOs;
using BarberOS.Application.Appointments.UseCases;
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
        [HttpGet("me/appointments")]
        [Authorize(Policy = Policies.BarberOnly)]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<AppointmentDto>>>> GetMyAppointments(
            [FromServices] ListBarberScheduleUseCase useCase,
            CancellationToken ct)
        {
            var result = await useCase.ExecuteAsync(ct);
            return Ok(ApiResponse<IReadOnlyList<AppointmentDto>>.Ok(result));
        }

        [HttpGet("me/balance")]
        [Authorize(Policy = Policies.BarberOnly)]
        public async Task<ActionResult<ApiResponse<BalanceDto>>> GetMyBalance(
            [FromServices] GetMyBalanceUseCase useCase,
            CancellationToken ct)
        {
            var result = await useCase.ExecuteAsync(ct);
            return Ok(ApiResponse<BalanceDto>.Ok(result));
        }

        [HttpGet]
        [Authorize(Policy = Policies.Management)]
        public async Task<ActionResult<ApiResponse<IReadOnlyList<BarberDto>>>> ListForAdmin(
            [FromQuery] Guid barbershopId,
            [FromServices] ListBarbersForAdminUseCase useCase,
            CancellationToken ct)
        {
            var result = await useCase.ExecuteAsync(barbershopId, ct);
            return Ok(ApiResponse<IReadOnlyList<BarberDto>>.Ok(result));
        }

        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<PublicBarberDto>>> GetById(
            Guid id,
            [FromServices] GetBarberByIdUseCase useCase,
            CancellationToken ct)
        {
            var result = await useCase.ExecuteAsync(id, ct);
            return Ok(ApiResponse<PublicBarberDto>.Ok(result));
        }

        [HttpPost]
        [Authorize(Policy = Policies.Management)]
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

        [HttpPost("onboard")]
        [Authorize(Policy = Policies.Management)]
        public async Task<ActionResult<ApiResponse<BarberDto>>> Onboard(
            [FromBody] OnboardBarberRequest request,
            [FromServices] IValidator<OnboardBarberRequest> validator,
            [FromServices] OnboardBarberUseCase useCase,
            CancellationToken ct)
        {
            await validator.ValidateAndThrowAsync(request, ct);
            var result = await useCase.ExecuteAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<BarberDto>.Ok(result, "Barbero dado de alta."));
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
