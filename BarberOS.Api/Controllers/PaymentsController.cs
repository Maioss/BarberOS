using BarberOS.Api.Common;
using BarberOS.Application.Payments.DTOs;
using BarberOS.Application.Payments.UseCases;
using BarberOS.Application.Shared;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberOS.Api.Controllers
{
    [ApiController]
    [Route("api/payments")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly ICurrentUserService _currentUser;

        public PaymentsController(ICurrentUserService currentUser) =>
            _currentUser = currentUser;

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterPaymentRequest request,
            [FromServices] IValidator<RegisterPaymentRequest> validator,
            [FromServices] RegisterPaymentUseCase useCase,
            CancellationToken ct)
        {
            await validator.ValidateAndThrowAsync(request, ct);
            var result = await useCase.ExecuteAsync(request, ct);
            return Created($"/api/payments/{result.Id}", ApiResponse<PaymentDto>.Ok(result, "Pago registrado."));
        }

        [HttpGet("me")]
        [Authorize(Roles = "Client")]
        public async Task<ActionResult<ApiResponse<PagedResult<PaymentDto>>>> GetMine(
            [FromQuery] PaymentFilter filter,
            [FromServices] ListMyPaymentsUseCase useCase,
            CancellationToken ct)
        {
            var userId = _currentUser.UserId!.Value;
            var result = await useCase.ExecuteAsync(userId, filter, ct);
            return Ok(ApiResponse<PagedResult<PaymentDto>>.Ok(result));
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<ApiResponse<PagedResult<PaymentDto>>>> List(
            [FromQuery] PaymentFilter filter,
            [FromServices] ListPaymentsUseCase useCase,
            CancellationToken ct)
        {
            var result = await useCase.ExecuteAsync(filter, ct);
            return Ok(ApiResponse<PagedResult<PaymentDto>>.Ok(result));
        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<ApiResponse<PaymentDto>>> GetById(
            Guid id,
            [FromServices] GetPaymentByIdUseCase useCase,
            CancellationToken ct)
        {
            var result = await useCase.ExecuteAsync(id, ct);
            return Ok(ApiResponse<PaymentDto>.Ok(result));
        }

        [HttpPatch("{id:guid}/refund")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Refund(
            Guid id,
            [FromServices] RefundPaymentUseCase useCase,
            CancellationToken ct)
        {
            await useCase.ExecuteAsync(id, ct);
            return NoContent();
        }
    }
}
