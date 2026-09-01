using BarberOS.Api.Common;
using BarberOS.Application.Auth.DTOs;
using BarberOS.Application.Auth.UseCases;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BarberOS.Api.Controllers
{

    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting(AuthRateLimit.PolicyName)]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequest request,
            [FromServices] IValidator<LoginRequest> validator,
            [FromServices] LoginUseCase useCase,
            CancellationToken ct)
        {
            await validator.ValidateAndThrowAsync(request, ct);
            var result = await useCase.ExecuteAsync(request, ct);
            return Ok(ApiResponse<AuthResponse>.Ok(result));
        }

        [HttpPost("register")]
        [AllowAnonymous]
        [EnableRateLimiting(AuthRateLimit.PolicyName)]
        public async Task<IActionResult> Register(
            [FromBody] RegisterClientRequest request,
            [FromServices] IValidator<RegisterClientRequest> validator,
            [FromServices] RegisterClientUseCase useCase,
            CancellationToken ct)
        {
            await validator.ValidateAndThrowAsync(request, ct);
            var result = await useCase.ExecuteAsync(request, ct);
            return Ok(ApiResponse<AuthResponse>.Ok(result, "Cuenta creada exitosamente."));
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        [EnableRateLimiting(AuthRateLimit.PolicyName)]
        public async Task<IActionResult> Refresh(
            [FromBody] RefreshSessionRequest request,
            [FromServices] RefreshSessionUseCase useCase,
            CancellationToken ct)
        {
            var result = await useCase.ExecuteAsync(request, ct);
            return Ok(ApiResponse<AuthResponse>.Ok(result));
        }

        [HttpPost("logout")]
        [AllowAnonymous]
        public async Task<IActionResult> Logout(
            [FromBody] RefreshSessionRequest request,
            [FromServices] LogoutUseCase useCase,
            CancellationToken ct)
        {
            await useCase.ExecuteAsync(request, ct);
            return NoContent();
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me(
            [FromServices] GetCurrentUserUseCase useCase,
            CancellationToken ct)
        {
            var result = await useCase.ExecuteAsync(ct);
            return Ok(ApiResponse<UserInfo>.Ok(result));
        }
    }
}
