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
        private readonly ICurrentUserService _currentUser;

        public AppointmentsController(ICurrentUserService currentUser) =>
            _currentUser = currentUser;

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin,Client")]
        public async Task<IActionResult> Create(
            [FromBody] CreateAppointmentRequest request,
            [FromServices] IValidator<CreateAppointmentRequest> validator,
            [FromServices] CreateAppointmentUseCase useCase,
            CancellationToken ct)
        {
            await validator.ValidateAndThrowAsync(request, ct);
            var userId = _currentUser.UserId!.Value;
            var role = _currentUser.Role!.Value;
            var result = await useCase.ExecuteAsync(userId, role, request, ct);
            return Created($"/api/appointments/{result.Id}", ApiResponse<AppointmentDto>.Ok(result, "Reserva creada."));
        }

        [HttpGet("me")]
        [Authorize(Roles = "Client")]
        public async Task<ActionResult<ApiResponse<PagedResult<AppointmentDto>>>> GetMine(
            [FromQuery] AppointmentFilter filter,
            [FromServices] ListMyAppointmentsUseCase useCase,
            CancellationToken ct)
        {
            var userId = _currentUser.UserId!.Value;
            var result = await useCase.ExecuteAsync(userId, filter, ct);
            return Ok(ApiResponse<PagedResult<AppointmentDto>>.Ok(result));
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<ApiResponse<PagedResult<AppointmentDto>>>> List(
            [FromQuery] AppointmentFilter filter,
            [FromServices] ListAppointmentsUseCase useCase,
            CancellationToken ct)
        {
            var result = await useCase.ExecuteAsync(filter, ct);
            return Ok(ApiResponse<PagedResult<AppointmentDto>>.Ok(result));
        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<ApiResponse<AppointmentDto>>> GetById(
            Guid id,
            [FromServices] GetAppointmentByIdUseCase useCase,
            CancellationToken ct)
        {
            var result = await useCase.ExecuteAsync(id, ct);
            return Ok(ApiResponse<AppointmentDto>.Ok(result));
        }

        [HttpPatch("{id:guid}/cancel")]
        [Authorize(Roles = "SuperAdmin,Admin,Client,Barber")]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromServices] CancelAppointmentUseCase useCase,
            CancellationToken ct)
        {
            var userId = _currentUser.UserId!.Value;
            var role = _currentUser.Role!.Value;
            await useCase.ExecuteAsync(id, userId, role, ct);
            return NoContent();
        }

        [HttpPatch("{id:guid}/complete")]
        [Authorize(Roles = "SuperAdmin,Admin,Barber")]
        public async Task<IActionResult> Complete(
            Guid id,
            [FromServices] CompleteAppointmentUseCase useCase,
            CancellationToken ct)
        {
            var userId = _currentUser.UserId!.Value;
            var role = _currentUser.Role!.Value;
            await useCase.ExecuteAsync(id, userId, role, ct);
            return NoContent();
        }
    }
}
