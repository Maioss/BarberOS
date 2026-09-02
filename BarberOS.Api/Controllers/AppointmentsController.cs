using BarberOS.Api.Common;
using BarberOS.Application.Appointments.DTOs;
using BarberOS.Application.Appointments.UseCases;
using BarberOS.Application.Shared;
using BarberOS.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberOS.Api.Controllers
{
    [ApiController]
    [Route("api/appointments")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        [HttpPost]
        [Authorize(Policy = Policies.CanBook)]
        public async Task<IActionResult> Create(
            [FromBody] CreateAppointmentRequest request,
            [FromServices] IValidator<CreateAppointmentRequest> validator,
            [FromServices] CreateAppointmentUseCase useCase,
            CancellationToken ct)
        {
            await validator.ValidateAndThrowAsync(request, ct);
            var result = await useCase.ExecuteAsync(request, ct);
            return Created($"/api/appointments/{result.Id}", ApiResponse<AppointmentDto>.Ok(result, "Reserva creada."));
        }

        [HttpGet("me")]
        [Authorize(Policy = Policies.ClientOnly)]
        public async Task<ActionResult<ApiResponse<PagedResult<AppointmentDto>>>> GetMine(
            [FromQuery] AppointmentFilter filter,
            [FromServices] ListMyAppointmentsUseCase useCase,
            CancellationToken ct)
        {
            var result = await useCase.ExecuteAsync(filter, ct);
            return Ok(ApiResponse<PagedResult<AppointmentDto>>.Ok(result));
        }

        [HttpGet]
        [Authorize(Policy = Policies.Management)]
        public async Task<ActionResult<ApiResponse<PagedResult<AppointmentDto>>>> List(
            [FromQuery] AppointmentFilter filter,
            [FromServices] ListAppointmentsUseCase useCase,
            CancellationToken ct)
        {
            var result = await useCase.ExecuteAsync(filter, ct);
            return Ok(ApiResponse<PagedResult<AppointmentDto>>.Ok(result));
        }

        [HttpGet("{id:guid}")]
        [Authorize(Policy = Policies.Management)]
        public async Task<ActionResult<ApiResponse<AppointmentDto>>> GetById(
            Guid id,
            [FromServices] GetAppointmentByIdUseCase useCase,
            CancellationToken ct)
        {
            var result = await useCase.ExecuteAsync(id, ct);
            return Ok(ApiResponse<AppointmentDto>.Ok(result));
        }

        [HttpPatch("{id:guid}/cancel")]
        [Authorize(Policy = Policies.CanCancel)]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromServices] CancelAppointmentUseCase useCase,
            CancellationToken ct)
        {
            await useCase.ExecuteAsync(id, ct);
            return NoContent();
        }

        [HttpPatch("{id:guid}/complete")]
        [Authorize(Policy = Policies.CanComplete)]
        public async Task<IActionResult> Complete(
            Guid id,
            [FromServices] CompleteAppointmentUseCase useCase,
            CancellationToken ct)
        {
            await useCase.ExecuteAsync(id, ct);
            return NoContent();
        }
    }
}
