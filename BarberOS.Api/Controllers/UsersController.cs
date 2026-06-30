using BarberOS.Api.Common;
using BarberOS.Application.Shared;
using BarberOS.Application.Users.DTOs;
using BarberOS.Application.Users.UseCases;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BarberOS.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetById(
            Guid id,
            [FromServices] GetUserByIdUseCase useCase,
            CancellationToken ct)
        {
            var result = await useCase.ExecuteAsync(id, ct);
            return Ok(ApiResponse<UserDto>.Ok(result));
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<ApiResponse<PagedResult<UserDto>>>> List(
            [FromQuery] UserFilter filter,
            [FromServices] ListUsersUseCase useCase,
            CancellationToken ct)
        {
            var result = await useCase.ExecuteAsync(filter, ct);
            return Ok(ApiResponse<PagedResult<UserDto>>.Ok(result));
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<ApiResponse<UserDto>>> Create(
            [FromBody] CreateUserRequest request,
            [FromServices] IValidator<CreateUserRequest> validator,
            [FromServices] CreateUserUseCase useCase,
            CancellationToken ct)
        {
            await validator.ValidateAndThrowAsync(request, ct);
            var result = await useCase.ExecuteAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, ApiResponse<UserDto>.Ok(result, "Usuario creado."));
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<ActionResult<ApiResponse<UserDto>>> Update(
            Guid id,
            [FromBody] UpdateUserRequest request,
            [FromServices] IValidator<UpdateUserRequest> validator,
            [FromServices] UpdateUserUseCase useCase,
            CancellationToken ct)
        {
            await validator.ValidateAndThrowAsync(request, ct);
            var result = await useCase.ExecuteAsync(id, request, ct);
            return Ok(ApiResponse<UserDto>.Ok(result, "Usuario actualizado."));
        }

        [HttpPut("me")]
        public async Task<ActionResult<ApiResponse<UserDto>>> UpdateMyProfile(
            [FromBody] UpdateMyProfileRequest request,
            [FromServices] IValidator<UpdateMyProfileRequest> validator,
            [FromServices] UpdateMyProfileUseCase useCase,
            CancellationToken ct)
        {
            await validator.ValidateAndThrowAsync(request, ct);
            var result = await useCase.ExecuteAsync(request, ct);
            return Ok(ApiResponse<UserDto>.Ok(result, "Perfil actualizado."));
        }

        [HttpPut("me/photo")]
        public async Task<ActionResult<ApiResponse<UserDto>>> UpdateMyPhoto(
            [FromBody] UpdateMyPhotoRequest request,
            [FromServices] UpdateMyPhotoUseCase useCase,
            CancellationToken ct)
        {
            var result = await useCase.ExecuteAsync(request, ct);
            return Ok(ApiResponse<UserDto>.Ok(result, "Foto actualizada."));
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Delete(
            Guid id,
            [FromServices] DeleteUserUseCase useCase,
            CancellationToken ct)
        {
            await useCase.ExecuteAsync(id, ct);
            return NoContent();
        }
    }
}
