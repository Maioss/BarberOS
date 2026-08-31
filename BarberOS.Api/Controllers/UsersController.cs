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
        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetMyProfile(
            [FromServices] GetMyProfileUseCase useCase,
            CancellationToken ct)
        {
            var result = await useCase.ExecuteAsync(ct);
            return Ok(ApiResponse<UserDto>.Ok(result));
        }

        [HttpGet("{id:guid}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
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

        [HttpPost("me/photo/upload")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ApiResponse<UserDto>>> UploadMyPhoto(
            IFormFile file,
            [FromServices] UpdateMyPhotoUseCase useCase,
            [FromServices] Microsoft.AspNetCore.Hosting.IWebHostEnvironment env,
            CancellationToken ct)
        {
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                return BadRequest(ApiResponse<UserDto>.Fail("Solo se permiten imágenes JPEG, PNG o WebP."));

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(ApiResponse<UserDto>.Fail("La imagen no puede superar 5 MB."));

            var ext = file.ContentType.ToLower() switch
            {
                "image/jpeg" => ".jpg",
                "image/png"  => ".png",
                "image/webp" => ".webp",
                _            => ".jpg"
            };

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.NewGuid().ToString();
            var fileName = $"{userId}{ext}";
            var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
            var photosDir = Path.Combine(webRoot, "photos");
            Directory.CreateDirectory(photosDir);

            using (var stream = System.IO.File.Create(Path.Combine(photosDir, fileName)))
                await file.CopyToAsync(stream, ct);

            var photoUrl = $"{Request.Scheme}://{Request.Host}/photos/{fileName}";
            var result = await useCase.ExecuteAsync(new UpdateMyPhotoRequest(photoUrl), ct);
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
