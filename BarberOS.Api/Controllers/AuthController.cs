using BarberOS.Api.Common;
using BarberOS.Application.Auth.DTOs;
using BarberOS.Application.Auth.UseCases;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberOS.Api.Controllers
{

    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        [AllowAnonymous]
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
